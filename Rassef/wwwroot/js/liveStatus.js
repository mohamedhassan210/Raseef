document.addEventListener('DOMContentLoaded', () => {

    const trackingCode = window.trackingCode;
    const pollUrl = window.routes?.getLiveStatus;
    const hubUrl = window.routes?.queueHub;

    const elLoading = document.getElementById('state-loading');
    const elNotFound = document.getElementById('state-not-found');
    const elWaiting = document.getElementById('state-waiting');
    const elYourTurn = document.getElementById('state-your-turn');
    const elDone = document.getElementById('state-done');

    const elTicketNumWaiting = document.getElementById('ls-ticket-num-waiting');
    const elWaitingCount = document.getElementById('ls-waiting-count');
    const elDockWaiting = document.getElementById('ls-dock-waiting');

    const elTicketNumTurn = document.getElementById('ls-ticket-num-turn');
    const elDockTurn = document.getElementById('ls-dock-turn');

    const elTicketNumDone = document.getElementById('ls-ticket-num-done');

    const allStates = [elLoading, elNotFound, elWaiting, elYourTurn, elDone];

    let stoppedPolling = false;
    let fallbackTimer = null;

    function showOnly(el) {
        allStates.forEach(s => {
            if (!s) return;
            s.classList.toggle('d-none', s !== el);
        });
    }

    function stopUpdates() {
        stoppedPolling = true;
        if (fallbackTimer) clearInterval(fallbackTimer);
    }

    async function fetchStatus() {
        if (stoppedPolling || !trackingCode || !pollUrl) return;

        try {
            const res = await fetch(`${pollUrl}?code=${encodeURIComponent(trackingCode)}`, {
                cache: 'no-store'
            });
            const data = await res.json();

            if (!data.found) {
                showOnly(elNotFound);
                stopUpdates();
                return;
            }

            if (data.state === 'waiting') {
                elTicketNumWaiting.textContent = data.ticketNumber || '--';
                elWaitingCount.textContent = data.waitingCount ?? '0';
                elDockWaiting.textContent = data.dockName || '--';
                showOnly(elWaiting);
            } else if (data.state === 'in_progress') {
                elTicketNumTurn.textContent = data.ticketNumber || '--';
                elDockTurn.textContent = data.dockName || '--';
                showOnly(elYourTurn);
                // نبضة اهتزاز بسيطة على الموبايل لو مدعومة، لتنبيه السواق
                if (window.navigator && window.navigator.vibrate) {
                    window.navigator.vibrate([200, 100, 200]);
                }
            } else if (data.state === 'done') {
                elTicketNumDone.textContent = data.ticketNumber || '--';
                showOnly(elDone);
                stopUpdates(); // الدور خلص، مفيش داعي نكمل نستقبل تحديثات
            }
        } catch (err) {
            console.error('تعذر تحديث حالة الدور:', err);
        }
    }

    // أول تحميل للصفحة: هات آخر حالة فوراً
    fetchStatus();

    // شبكة أمان: لو الاتصال بالـ Hub اتقطع لأي سبب، نفضل نسأل كل 20 ثانية
    fallbackTimer = setInterval(fetchStatus, 20000);

    // الاتصال بالـ SignalR Hub: أي تغيير في أي دور بيوصلنا فوراً بدل ما ننتظر
    if (hubUrl && window.signalR) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();

        connection.on('QueueUpdated', () => {
            fetchStatus();
        });

        connection.start().catch(err => {
            // لو فشل الاتصال بالـ Hub (مثلاً مشكلة شبكة)، هنفضل شغالين
            // على شبكة الأمان (fetchStatus كل 20 ثانية) لحد ما يترظبط تاني
            console.error('تعذر الاتصال بخدمة التحديث اللحظي:', err);
        });
    }
});

