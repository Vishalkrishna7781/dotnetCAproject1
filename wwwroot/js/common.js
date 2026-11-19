// Shared helpers: local privacy log and online/offline indicator. All data stays in this browser.
window.HC = (function () {
  var LOG_KEY = 'hc.privacyLog';
  function read(key, fallback) {
    try { var v = localStorage.getItem(key); return v ? JSON.parse(v) : fallback; } catch (e) { return fallback; }
  }
  function write(key, value) {
    try { localStorage.setItem(key, JSON.stringify(value)); } catch (e) { /* storage unavailable */ }
  }
  function log(text) {
    var entries = read(LOG_KEY, []);
    entries.unshift({ t: new Date().toISOString(), text: text });
    write(LOG_KEY, entries.slice(0, 50));
  }
  var net = document.getElementById('netState');
  function updateNet() {
    if (!net) return;
    var on = navigator.onLine;
    net.textContent = on ? 'Online (not required)' : 'Offline: fully working';
  }
  window.addEventListener('online', updateNet);
  window.addEventListener('offline', updateNet);
  updateNet();
  return { read: read, write: write, log: log, LOG_KEY: LOG_KEY };
})();
