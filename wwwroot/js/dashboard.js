(function () {
  var SCENARIOS = {
    normal:  { heartRate: 74,  spo2: 97, temperature: 36.8, systolic: 118, diastolic: 76, respRate: 15, activity: 'rest' },
    heat:    { heartRate: 128, spo2: 95, temperature: 39.8, systolic: 108, diastolic: 70, respRate: 26, activity: 'rest' },
    hypoxia: { heartRate: 112, spo2: 87, temperature: 37.0, systolic: 124, diastolic: 80, respRate: 28, activity: 'rest' },
    shock:   { heartRate: 124, spo2: 93, temperature: 36.0, systolic: 82,  diastolic: 50, respRate: 24, activity: 'rest' }
  };
  var CONFIRM = 3;          // consecutive readings needed to escalate
  var MAX_POINTS = 60;
  var ORDER = { Normal: 0, Warning: 1, Critical: 2 };

  var $ = function (id) { return document.getElementById(id); };
  var state = { running: true, level: 'Normal', pendingLevel: 'Normal', pendingCount: 0, hr: [], current: null, timer: null };

  function jitter(v, amt, dec) {
    var n = v + (Math.random() * 2 - 1) * amt;
    var f = Math.pow(10, dec || 0);
    return Math.round(n * f) / f;
  }

  function nextReading() {
    var key = $('scenario').value;
    if (key === 'manual') return state.current;
    var b = SCENARIOS[key];
    return {
      heartRate: jitter(b.heartRate, 3), spo2: Math.min(100, jitter(b.spo2, 1)),
      temperature: jitter(b.temperature, 0.1, 1), systolic: jitter(b.systolic, 3),
      diastolic: jitter(b.diastolic, 2), respRate: jitter(b.respRate, 1), activity: b.activity
    };
  }

  function showVitals(r) {
    $('v-hr').textContent = r.heartRate;
    $('v-spo2').textContent = r.spo2;
    $('v-temp').textContent = r.temperature.toFixed(1);
    $('v-bp').textContent = r.systolic + '/' + r.diastolic;
    $('v-rr').textContent = r.respRate;
    $('v-act').textContent = r.activity;
  }

  function tileClasses(findings) {
    var map = { 'Heart rate': 't-hr', 'SpO2': 't-spo2', 'Temperature': 't-temp', 'Blood pressure': 't-bp', 'Respiratory rate': 't-rr' };
    Object.keys(map).forEach(function (k) { $(map[k]).className = 'tile'; });
    findings.forEach(function (f) {
      var id = map[f.vital];
      if (!id) return;
      var cur = $(id).className;
      if (f.level === 'Critical') $(id).className = 'tile critical';
      else if (cur.indexOf('critical') < 0) $(id).className = 'tile warning';
    });
  }

  function beep(level) {
    if (!$('sound').checked) return;
    try {
      var Ctx = window.AudioContext || window.webkitAudioContext;
      var ctx = new Ctx();
      var o = ctx.createOscillator(), g = ctx.createGain();
      o.frequency.value = level === 'Critical' ? 880 : 520;
      g.gain.value = 0.15;
      o.connect(g); g.connect(ctx.destination);
      o.start(); o.stop(ctx.currentTime + (level === 'Critical' ? 0.7 : 0.3));
    } catch (e) { /* audio blocked until user interacts */ }
  }

  function addTimeline(level, msg) {
    var ul = $('timeline');
    if (ul.firstElementChild && ul.firstElementChild.className === 'muted') ul.innerHTML = '';
    var li = document.createElement('li');
    li.className = level.toLowerCase();
    var t = document.createElement('time');
    t.textContent = new Date().toLocaleTimeString();
    var s = document.createElement('span');
    s.textContent = level + ': ' + msg;
    li.appendChild(t); li.appendChild(s);
    ul.insertBefore(li, ul.firstChild);
    while (ul.children.length > 30) ul.removeChild(ul.lastChild);
  }

  function renderRing(a) {
    var pct = Math.round((1 - a.modelScore) * 100);
    var ring = $('ring');
    var color = state.level === 'Critical' ? '#a8231c' : state.level === 'Warning' ? '#b9770e' : '#3f6b4f';
    ring.style.setProperty('--p', pct);
    ring.style.setProperty('--c', color);
    $('ringVal').textContent = pct + '%';
    ring.setAttribute('aria-label', 'Stability index ' + pct + ' percent');
  }

  function renderAlert(a) {
    renderRing(a);
    var box = $('alert');
    var lvl = state.level;
    box.className = 'alert ' + lvl.toLowerCase();
    $('alertLevel').textContent = lvl;
    var text = a.findings.length ? a.findings.map(function (f) { return f.message; }).join(' ') : 'No significant abnormality detected.';
    if (lvl === 'Normal') text = 'No significant abnormality detected.';
    $('alertText').textContent = text;
    var g = $('alertGuide');
    if (lvl === 'Normal') { g.hidden = true; }
    else { g.hidden = false; g.href = '/FirstAid#' + (a.guidance === 'none' ? 'monitor' : a.guidance); }
  }

  function applyLevel(a) {
    var incoming = a.level;
    if (ORDER[incoming] < ORDER[state.level]) {
      // De-escalate immediately only after the same confirmation window.
    }
    if (incoming === state.level) { state.pendingLevel = incoming; state.pendingCount = 0; renderAlert(a); return; }
    if (incoming === state.pendingLevel) state.pendingCount++;
    else { state.pendingLevel = incoming; state.pendingCount = 1; }
    if (state.pendingCount >= CONFIRM) {
      state.level = incoming; state.pendingCount = 0;
      addTimeline(incoming, a.findings.length ? a.findings[0].message : 'Readings back in normal range.');
      HC.log('Risk level changed to ' + incoming + ' (processed on device, not shared).');
      if (incoming !== 'Normal') beep(incoming);
    }
    renderAlert(a);
  }

  function drawSpark() {
    var c = $('spark'), g = c.getContext('2d');
    var w = c.width, h = c.height;
    g.clearRect(0, 0, w, h);
    var css = getComputedStyle(document.documentElement);
    g.strokeStyle = css.getPropertyValue('--line').trim() || '#ccc';
    g.lineWidth = 1;
    [60, 100, 140].forEach(function (v) {
      var y = h - ((v - 30) / 150) * h;
      g.beginPath(); g.moveTo(0, y); g.lineTo(w, y); g.stroke();
    });
    if (state.hr.length < 2) return;
    g.strokeStyle = css.getPropertyValue('--accent').trim() || '#0a6';
    g.lineWidth = 2.5;
    g.beginPath();
    state.hr.forEach(function (v, i) {
      var x = (i / (MAX_POINTS - 1)) * w;
      var y = h - ((Math.min(Math.max(v, 30), 180) - 30) / 150) * h;
      if (i === 0) g.moveTo(x, y); else g.lineTo(x, y);
    });
    g.stroke();
  }

  async function assess(r) {
    try {
      var res = await fetch('/api/assess', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(r) });
      if (!res.ok) { var e = await res.json(); throw new Error(e.error || 'Invalid reading'); }
      return await res.json();
    } catch (err) {
      $('alertText').textContent = 'Could not assess reading: ' + err.message;
      return null;
    }
  }

  async function tick() {
    if (!state.running) return;
    var r = nextReading();
    if (!r) return;
    state.current = r;
    showVitals(r);
    state.hr.push(r.heartRate);
    if (state.hr.length > MAX_POINTS) state.hr.shift();
    drawSpark();
    var a = await assess(r);
    if (a) { tileClasses(a.findings); applyLevel(a); state.last = a; }
  }

  $('toggle').addEventListener('click', function () {
    state.running = !state.running;
    this.textContent = state.running ? 'Pause' : 'Resume';
  });

  $('scenario').addEventListener('change', function () {
    state.pendingCount = 0;
    if (this.value === 'manual') { state.running = false; $('toggle').textContent = 'Resume'; }
    else { state.running = true; $('toggle').textContent = 'Pause'; }
  });

  $('manual').addEventListener('submit', async function (ev) {
    ev.preventDefault();
    var f = new FormData(ev.target);
    var r = {
      heartRate: +f.get('heartRate'), spo2: +f.get('spo2'), temperature: +f.get('temperature'),
      systolic: +f.get('systolic'), diastolic: +f.get('diastolic'), respRate: +f.get('respRate'), activity: f.get('activity')
    };
    $('scenario').value = 'manual';
    state.running = false; $('toggle').textContent = 'Resume';
    state.current = r; showVitals(r);
    var a = await assess(r);
    if (a) {
      // Manual entry is a deliberate check, so it takes effect immediately.
      tileClasses(a.findings);
      if (a.level !== state.level) {
        state.level = a.level;
        addTimeline(a.level, a.findings.length ? a.findings[0].message : 'Readings in normal range.');
        HC.log('Manual check: risk level ' + a.level + ' (processed on device, not shared).');
        if (a.level !== 'Normal') beep(a.level);
      }
      renderAlert(a);
    }
  });

  $('consent').addEventListener('change', function () { $('sosBtn').disabled = !this.checked; });
  $('sosBtn').addEventListener('click', function () {
    var med = HC.read('hc.medical', {});
    var who = med.contact ? ' to ' + med.contact : ' to your emergency contacts';
    $('sosMsg').textContent = 'Prototype: SOS (' + state.level + ' risk) would be sent' + who +
      ' by SMS or a nearby BLE / Wi-Fi Direct beacon when a connection is available.';
    HC.log('SOS shared with consent (simulated): risk level ' + state.level + '.');
  });

  state.timer = setInterval(tick, 2000);
  tick();
})();
