namespace Rassef.Common.Services
{
    public class TicketEngineService : ITicketEngineService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IQueueTicketRepository _ticketRepository;
        private readonly IRepository<QueueSettings> _queueSettingsRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IRepository<TicketStatuses> _ticketStatusRepository;
        private readonly IDockRepository _dockRepository;
        private readonly IDockAssignmentRepository _dockAssignmentRepository;
        private readonly IUserRepository _userRepository;

        // Concurrency Guard: منع أي Race Condition أثناء استدعاء الأدوار أو توليد التذاكر المتزامنة
        private static readonly System.Threading.SemaphoreSlim _concurrencyLock = new System.Threading.SemaphoreSlim(1, 1);

        public TicketEngineService(
            IDepartmentRepository departmentRepository,
            IQueueTicketRepository ticketRepository,
            IRepository<QueueSettings> queueSettingsRepository,
            IShiftRepository shiftRepository,
            IRepository<TicketStatuses> ticketStatusRepository,
            IDockRepository dockRepository,
            IDockAssignmentRepository dockAssignmentRepository,
            IUserRepository userRepository)
        {
            _departmentRepository = departmentRepository;
            _ticketRepository = ticketRepository;
            _queueSettingsRepository = queueSettingsRepository;
            _shiftRepository = shiftRepository;
            _ticketStatusRepository = ticketStatusRepository;
            _dockRepository = dockRepository;
            _dockAssignmentRepository = dockAssignmentRepository;
            _userRepository = userRepository;
        }

        public async Task<(string Prefix, int Counter, string TicketNumber, Shift? ActiveShift)> GenerateTicketNumberAsync(int departmentId)
        {
            var department = await _departmentRepository.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new ArgumentException($"Department with ID {departmentId} not found.");
            }

            string prefix = !string.IsNullOrWhiteSpace(department.Prefix)
                ? department.Prefix.Trim().ToUpper()
                : ((char)('A' + ((Math.Max(1, department.Id) - 1) % 26))).ToString();

            var settings = (await _queueSettingsRepository.GetAllAsync()).FirstOrDefault();
            var resetType = settings?.ResetType ?? ResetType.ByShift;

            Shift? activeShift = null;
            var now = DateTimeOffset.Now;
            DateTimeOffset resetDate = new DateTimeOffset(DateTime.Today, now.Offset);

            if (resetType == ResetType.ByShift)
            {
                if (settings?.ShiftId.HasValue == true && settings.ShiftId.Value > 0)
                {
                    activeShift = await _shiftRepository.GetByIdAsync(settings.ShiftId.Value);
                }
                else
                {
                    var allShifts = await _shiftRepository.GetAllAsync();
                    var currentTime = now.TimeOfDay;
                    activeShift = allShifts.FirstOrDefault(s =>
                    {
                        var start = s.StartTime;
                        var end = start.Add(s.Duration);
                        if (end.TotalHours <= 24)
                        {
                            return currentTime >= start && currentTime < end;
                        }
                        else
                        {
                            var endNextDay = end.Subtract(TimeSpan.FromHours(24));
                            return currentTime >= start || currentTime < endNextDay;
                        }
                    }) ?? allShifts.FirstOrDefault();
                }

                if (activeShift != null)
                {
                    var shiftStart = DateTime.Today.Add(activeShift.StartTime);
                    if (now.DateTime < shiftStart)
                    {
                        shiftStart = shiftStart.AddDays(-1);
                    }
                    resetDate = new DateTimeOffset(shiftStart, now.Offset);

                    if (activeShift.LastResetAt.HasValue && activeShift.LastResetAt.Value <= now && activeShift.LastResetAt.Value > resetDate)
                    {
                        resetDate = activeShift.LastResetAt.Value;
                    }
                }
            }
            else if (resetType == ResetType.Daily)
            {
                resetDate = new DateTimeOffset(DateTime.Today, now.Offset);
            }
            else // Manual
            {
                if (settings?.LastGlobalResetAt.HasValue == true && settings.LastGlobalResetAt.Value <= now)
                {
                    resetDate = settings.LastGlobalResetAt.Value;
                }
            }

            if (department.LastResetAt.HasValue && department.LastResetAt.Value <= now && department.LastResetAt.Value > resetDate)
            {
                resetDate = department.LastResetAt.Value;
            }

            if (settings?.LastGlobalResetAt.HasValue == true && settings.LastGlobalResetAt.Value <= now && settings.LastGlobalResetAt.Value > resetDate)
            {
                resetDate = settings.LastGlobalResetAt.Value;
            }

            var allTickets = await _ticketRepository.GetAllAsync();

            int maxCounter = 0;
            foreach (var t in allTickets)
            {
                if (t.DepartmentId == departmentId || (!string.IsNullOrWhiteSpace(t.TicketNumber) && t.TicketNumber.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    if (t.CreatedAT >= resetDate || t.QueueTime >= resetDate || t.CreatedAT >= new DateTimeOffset(DateTime.Today, now.Offset))
                    {
                        if (!string.IsNullOrWhiteSpace(t.TicketNumber))
                        {
                            string trimmed = t.TicketNumber.Trim();
                            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                var numPart = trimmed.Substring(prefix.Length).Trim();
                                if (int.TryParse(numPart, out var val) && val > maxCounter)
                                {
                                    maxCounter = val;
                                }
                            }
                            else
                            {
                                var digits = new string(trimmed.Where(char.IsDigit).ToArray());
                                if (int.TryParse(digits, out var val) && val > maxCounter && val < 100000)
                                {
                                    maxCounter = val;
                                }
                            }
                        }
                    }
                }
            }

            int nextCounter = maxCounter + 1;
            string ticketNumber = $"{prefix}{nextCounter}";

            return (prefix, nextCounter, ticketNumber, activeShift);
        }

        public async Task<TicketIssueResult> IssueTransferTicketAsync(int departmentId, int transferRequestId, int userId)
        {
            return await IssueGeneralTicketAsync(departmentId, supplierRequestId: null, transferRequestId: transferRequestId, ticketStatusId: null, userId: userId);
        }

        public async Task<TicketIssueResult> IssueSupplierTicketAsync(int departmentId, int supplierRequestId, int userId)
        {
            return await IssueGeneralTicketAsync(departmentId, supplierRequestId: supplierRequestId, transferRequestId: null, ticketStatusId: null, userId: userId);
        }

        public async Task<TicketIssueResult> IssueGeneralTicketAsync(int departmentId, int? supplierRequestId, int? transferRequestId, int? ticketStatusId, int userId)
        {
            await _concurrencyLock.WaitAsync();
            try
            {
                var department = await _departmentRepository.GetByIdAsync(departmentId);
                if (department == null)
                {
                    throw new ArgumentException($"Department with ID {departmentId} not found.");
                }

                var (prefix, counter, ticketNumber, activeShift) = await GenerateTicketNumberAsync(departmentId);

                var allStatuses = await _ticketStatusRepository.GetAllAsync();
                var status = ticketStatusId.HasValue && ticketStatusId.Value > 0
                    ? allStatuses.FirstOrDefault(s => s.Id == ticketStatusId.Value)
                    : allStatuses.FirstOrDefault(s => s.Name.Contains("انتظار") || s.Name.Contains("إنتظار")) ?? allStatuses.FirstOrDefault();

                var currentUser = await _userRepository.GetByIdAsync(userId);
                if (currentUser == null)
                {
                    var allUsers = await _userRepository.GetAllAsync();
                    currentUser = allUsers.FirstOrDefault();
                }

                var queueTicket = new QueueTicket
                {
                    TicketNumber = ticketNumber,
                    DepartmentId = departmentId,
                    TicketStatusId = status?.Id ?? 1,
                    SupplierRequestId = supplierRequestId,
                    TransferRequestId = transferRequestId,
                    ShiftId = activeShift?.Id,
                    QueueTime = DateTimeOffset.Now,
                    EntryTime = DateTimeOffset.Now,
                    ExitTime = DateTimeOffset.MinValue,
                    CreatedBy = currentUser!
                };

                await _ticketRepository.AddAsync(queueTicket);
                await _ticketRepository.SaveChangesAsync();

                // Assign to an available dock if one exists for the department
                string dockName = $"{prefix}1";
                var allDocks = await _dockRepository.GetAllAsync();
                var availableDock = allDocks.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (availableDock != null)
                {
                    dockName = availableDock.DockName;
                    var dockAssignment = new DockAssignment
                    {
                        DockId = availableDock.Id,
                        TicketId = queueTicket.Id,
                        AssignedAt = DateTimeOffset.Now,
                        CreatedBy = currentUser!
                    };
                    await _dockAssignmentRepository.AddAsync(dockAssignment);
                    await _dockAssignmentRepository.SaveChangesAsync();
                }

                var allTickets = await _ticketRepository.GetAllAsync();
                int waitingCount = allTickets.Count(t =>
                {
                    if (t.Id >= queueTicket.Id) return false;

                    bool matchesDept = t.DepartmentId == departmentId ||
                        (!string.IsNullOrWhiteSpace(t.TicketNumber) && !string.IsNullOrWhiteSpace(ticketNumber) &&
                         char.ToUpper(t.TicketNumber.Trim()[0]) == char.ToUpper(ticketNumber.Trim()[0]));

                    if (!matchesDept && departmentId > 0) return false;

                    if (t.ExitTime != DateTimeOffset.MinValue && t.ExitTime > t.QueueTime) return false;

                    if (t.TicketStatus != null)
                    {
                        var st = t.TicketStatus.Name.Replace("إ", "ا").Trim().ToLower();
                        if (st.Contains("مكتمل") || st.Contains("تم") || st.Contains("خروج") || st.Contains("منتهي"))
                            return false;
                    }

                    return true;
                });

                if (waitingCount == 0 && queueTicket.Id > 1)
                {
                    waitingCount = allTickets.Count(t =>
                        t.Id < queueTicket.Id &&
                        (t.ExitTime == DateTimeOffset.MinValue || t.ExitTime <= t.QueueTime) &&
                        (t.TicketStatus == null || (!t.TicketStatus.Name.Contains("مكتمل") && !t.TicketStatus.Name.Contains("تم") && !t.TicketStatus.Name.Contains("خروج")))
                    );
                }

                string employeeName = currentUser?.Name
                    ?? (!string.IsNullOrWhiteSpace(currentUser?.UserName) ? currentUser.UserName : "المسؤول");

                return new TicketIssueResult
                {
                    Ticket = queueTicket,
                    TicketNumber = ticketNumber,
                    DepartmentName = department.Name,
                    Prefix = prefix,
                    SequenceNumber = counter,
                    DockName = dockName,
                    EmployeeName = employeeName,
                    WaitingCount = waitingCount
                };
            }
            finally
            {
                _concurrencyLock.Release();
            }
        }

        public async Task<TicketStatusUpdateResult> CallNextTicketAsync(int? departmentId, int userId)
        {
            await _concurrencyLock.WaitAsync();
            try
            {
                var allStatuses = await _ticketStatusRepository.GetAllAsync();
                var inProgressStatus = allStatuses.FirstOrDefault(s =>
                {
                    var n = s.Name.Replace("إ", "ا").ToLower();
                    return n.Contains("جاري") || n.Contains("تشغيل") || n.Contains("تنفيذ") || n.Contains("progress") || n.Contains("active");
                }) ?? allStatuses.FirstOrDefault(s => s.Id == 2);

                var completedStatus = allStatuses.FirstOrDefault(s =>
                {
                    var n = s.Name.Replace("إ", "ا").ToLower();
                    return n.Contains("مكتمل") || n.Contains("تم") || n.Contains("خروج") || n.Contains("منتهي") || n.Contains("complete") || n.Contains("done");
                }) ?? allStatuses.FirstOrDefault(s => s.Id == 3);

                var waitingStatus = allStatuses.FirstOrDefault(s =>
                {
                    var n = s.Name.Replace("إ", "ا").ToLower();
                    return n.Contains("انتظار") || n.Contains("طابور") || n.Contains("معلق") || n.Contains("wait") || n.Contains("pending");
                }) ?? allStatuses.FirstOrDefault(s => s.Id == 1);

                var tickets = await _ticketRepository.GetAllAsync(query => query
                    .Include(t => t.Department)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.SupplierRequest)
                        .ThenInclude(sr => sr.Driver)
                    .Include(t => t.SupplierRequest)
                        .ThenInclude(sr => sr.Truck)
                    .Include(t => t.SupplierRequest)
                        .ThenInclude(sr => sr.Supplier)
                    .Include(t => t.TransferRequest)
                        .ThenInclude(tr => tr.Driver)
                    .Include(t => t.TransferRequest)
                        .ThenInclude(tr => tr.Truck)
                    .Include(t => t.DockAssignments)
                        .ThenInclude(da => da.Dock)
                );

                var waitingTickets = tickets
                    .Where(t =>
                    {
                        if (completedStatus != null && t.TicketStatusId == completedStatus.Id) return false;
                        if (inProgressStatus != null && t.TicketStatusId == inProgressStatus.Id) return false;
                        if (t.ExitTime != DateTimeOffset.MinValue && t.ExitTime > t.QueueTime) return false;

                        if (t.TicketStatus != null)
                        {
                            var stName = t.TicketStatus.Name.Replace("إ", "ا").ToLower();
                            if (stName.Contains("جاري") || stName.Contains("مكتمل") || stName.Contains("تم") || stName.Contains("خروج"))
                                return false;
                        }

                        if (departmentId.HasValue && departmentId.Value > 0 && t.DepartmentId != departmentId.Value)
                            return false;

                        return true;
                    })
                    .OrderBy(t => t.QueueTime)
                    .ThenBy(t => t.Id)
                    .ToList();

                // 1. فحص وإنهاء أي دور كان قيد التنفيذ حالياً (تحويله إلى مكتمل وإطلاق سراح السائق والشاحنة)
                var currentInProgressTickets = tickets
                    .Where(t =>
                    {
                        if (departmentId.HasValue && departmentId.Value > 0 && t.DepartmentId != departmentId.Value)
                            return false;
                        if (t.ExitTime != DateTimeOffset.MinValue && t.ExitTime > t.QueueTime) return false;
                        if (t.TicketStatus != null)
                        {
                            var stName = t.TicketStatus.Name.Replace("إ", "ا").ToLower();
                            return stName.Contains("جاري") || stName.Contains("تنفيذ") || stName.Contains("تشغيل");
                        }
                        return t.TicketStatusId == (inProgressStatus?.Id ?? 2);
                    })
                    .ToList();

                foreach (var cur in currentInProgressTickets)
                {
                    var dbCur = await _ticketRepository.GetByIdAsync(cur.Id);
                    if (dbCur != null)
                    {
                        dbCur.TicketStatusId = completedStatus?.Id ?? 3;
                        dbCur.ExitTime = DateTimeOffset.Now;
                        dbCur.MarkAsUpdated();
                        _ticketRepository.Update(dbCur);
                    }
                }
                if (currentInProgressTickets.Any())
                {
                    await _ticketRepository.SaveChangesAsync();
                }

                // 2. فحص استدعاء الدور التالي من قائمة الانتظار
                if (waitingTickets.Any())
                {
                    var nextTicket = waitingTickets.First();
                    var ticketToUpdate = await _ticketRepository.GetByIdAsync(nextTicket.Id);
                    if (ticketToUpdate != null)
                    {
                        ticketToUpdate.TicketStatusId = inProgressStatus?.Id ?? 2;
                        ticketToUpdate.EntryTime = DateTimeOffset.Now;
                        ticketToUpdate.MarkAsUpdated();
                        _ticketRepository.Update(ticketToUpdate);
                        await _ticketRepository.SaveChangesAsync();
                    }

                    var dockAssignment = nextTicket.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
                    string truckPlate = nextTicket.SupplierRequest?.Truck != null
                        ? $"{nextTicket.SupplierRequest.Truck.PlateLetter} {nextTicket.SupplierRequest.Truck.PlateNumber}"
                        : (nextTicket.TransferRequest?.Truck != null ? $"{nextTicket.TransferRequest.Truck.PlateLetter} {nextTicket.TransferRequest.Truck.PlateNumber}" : "غير محدد");

                    string driverName = nextTicket.SupplierRequest?.Driver?.FullName
                        ?? nextTicket.TransferRequest?.Driver?.FullName ?? "غير محدد";

                    return new TicketStatusUpdateResult
                    {
                        Success = true,
                        Message = $"تم اكتمال الدور السابق واستدعاء الدور التالي {nextTicket.TicketNumber} للشاحنة {truckPlate} بنجاح.",
                        TicketId = nextTicket.Id,
                        TicketNumber = nextTicket.TicketNumber,
                        NewStatus = inProgressStatus?.Name ?? "جاري التنفيذ",
                        TruckPlate = truckPlate,
                        DriverName = driverName,
                        DepartmentName = nextTicket.Department?.Name ?? "غير محدد",
                        DockName = dockAssignment?.Dock?.DockName ?? "A1"
                    };
                }
                else if (currentInProgressTickets.Any())
                {
                    return new TicketStatusUpdateResult
                    {
                        Success = true,
                        Message = "تم اكتمال وإنهاء الدور السابق بنجاح. لا توجد أدوار متبقية في قائمة الانتظار.",
                        NewStatus = completedStatus?.Name ?? "مكتمل"
                    };
                }
                else
                {
                    return new TicketStatusUpdateResult
                    {
                        Success = false,
                        Message = "لا يوجد أي أدوار في قائمة الانتظار حالياً."
                    };
                }
            }
            finally
            {
                _concurrencyLock.Release();
            }
        }

        public async Task<TicketStatusUpdateResult> UpdateTicketStatusAsync(int ticketId, string targetStatusName, int userId)
        {
            await _concurrencyLock.WaitAsync();
            try
            {
                var ticketToUpdate = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticketToUpdate == null)
                {
                    return new TicketStatusUpdateResult
                    {
                        Success = false,
                        Message = "التذكرة غير موجودة."
                    };
                }

                var allStatuses = await _ticketStatusRepository.GetAllAsync();
                var targetStatus = allStatuses.FirstOrDefault(s => s.Name.Equals(targetStatusName, StringComparison.OrdinalIgnoreCase) || s.Name.Contains(targetStatusName));

                if (targetStatus == null)
                {
                    targetStatus = allStatuses.FirstOrDefault(s => targetStatusName.Contains("مكتمل") && (s.Name.Contains("مكتمل") || s.Name.Contains("تم") || s.Name.Contains("خروج")))
                        ?? allStatuses.FirstOrDefault(s => targetStatusName.Contains("جاري") && (s.Name.Contains("جاري") || s.Name.Contains("تنفيذ")))
                        ?? allStatuses.FirstOrDefault(s => targetStatusName.Contains("انتظار") && s.Name.Contains("انتظار"));
                }

                if (targetStatus != null)
                {
                    ticketToUpdate.TicketStatusId = targetStatus.Id;
                }

                var norm = targetStatusName.Replace("إ", "ا").Trim();
                if (norm.Contains("مكتمل") || norm.Contains("تم") || norm.Contains("خروج") || norm.Contains("منتهي"))
                {
                    ticketToUpdate.ExitTime = DateTimeOffset.Now;
                }
                else if (norm.Contains("جاري") || norm.Contains("تنفيذ") || norm.Contains("تشغيل"))
                {
                    ticketToUpdate.EntryTime = DateTimeOffset.Now;
                }

                ticketToUpdate.MarkAsUpdated();
                _ticketRepository.Update(ticketToUpdate);
                await _ticketRepository.SaveChangesAsync();

                var tickets = await _ticketRepository.GetAllAsync(query => query
                    .Where(t => t.Id == ticketId)
                    .Include(t => t.Department)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.SupplierRequest)
                        .ThenInclude(sr => sr.Driver)
                    .Include(t => t.SupplierRequest)
                        .ThenInclude(sr => sr.Truck)
                    .Include(t => t.TransferRequest)
                        .ThenInclude(tr => tr.Driver)
                    .Include(t => t.TransferRequest)
                        .ThenInclude(tr => tr.Truck)
                    .Include(t => t.DockAssignments)
                        .ThenInclude(da => da.Dock)
                );

                var ticketDetails = tickets.FirstOrDefault();
                var dockAssignment = ticketDetails?.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
                string truckPlate = ticketDetails?.SupplierRequest?.Truck != null
                    ? $"{ticketDetails.SupplierRequest.Truck.PlateLetter} {ticketDetails.SupplierRequest.Truck.PlateNumber}"
                    : (ticketDetails?.TransferRequest?.Truck != null ? $"{ticketDetails.TransferRequest.Truck.PlateLetter} {ticketDetails.TransferRequest.Truck.PlateNumber}" : "غير محدد");

                string driverName = ticketDetails?.SupplierRequest?.Driver?.FullName
                    ?? ticketDetails?.TransferRequest?.Driver?.FullName ?? "غير محدد";

                return new TicketStatusUpdateResult
                {
                    Success = true,
                    Message = $"تم تغيير حالة الدور {ticketToUpdate.TicketNumber} إلى {targetStatus?.Name ?? targetStatusName}.",
                    TicketId = ticketToUpdate.Id,
                    TicketNumber = ticketToUpdate.TicketNumber,
                    NewStatus = targetStatus?.Name ?? targetStatusName,
                    TruckPlate = truckPlate,
                    DriverName = driverName,
                    DepartmentName = ticketDetails?.Department?.Name ?? "غير محدد",
                    DockName = dockAssignment?.Dock?.DockName ?? "A1"
                };
            }
            finally
            {
                _concurrencyLock.Release();
            }
        }
    }
}
