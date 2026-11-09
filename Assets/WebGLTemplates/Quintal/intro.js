(() => {
  'use strict';
  const overlay = document.querySelector('#intro');
  const video = document.querySelector('#intro-video');
  const sound = document.querySelector('#intro-sound');
  const skip = document.querySelector('#intro-skip');
  const canvas = document.querySelector('#unity-canvas');
  let instance, request, started = false, finished = false, watchdog;
  function finish() {
    if (finished || !started) return;
    finished = true;
    clearTimeout(watchdog);
    video.pause();
    video.removeAttribute('src');
    video.load();
    overlay.hidden = true;
    canvas.inert = false;
    instance.SendMessage(request.receiver, 'CompleteIntro');
    canvas.focus();
  }
  function attemptPlay(muted = false) {
    if (finished) return;
    video.muted = muted;
    sound.hidden = !muted || video.volume === 0;
    clearTimeout(watchdog);
    watchdog = setTimeout(finish, 20000);
    const attempt = video.play();
    if (attempt) attempt.catch(error => {
      if (finished) return;
      // Audible autoplay can be blocked; continue immediately without audio.
      if (error.name === 'NotAllowedError' && !muted) attemptPlay(true);
      else finish();
    });
  }
  function start() {
    if (!instance || !request || started) return;
    started = true;
    canvas.inert = true;
    canvas.blur();
    overlay.hidden = false;
    video.volume = Math.max(0, Math.min(1, request.volume));
    if (video.error) { finish(); return; }
    skip.focus();
    attemptPlay();
  }
  window.startZHoldIntro = (receiver, volume) => { request = { receiver, volume }; start(); };
  window.zholdIntroReady = game => { instance = game; start(); };
  sound.addEventListener('click', () => attemptPlay(false));
  skip.addEventListener('click', finish);
  video.addEventListener('ended', finish);
  video.addEventListener('error', finish);
  video.addEventListener('playing', () => {
    clearTimeout(watchdog);
    watchdog = setTimeout(finish, Math.max(20, video.duration + 10) * 1000);
  });
  document.addEventListener('keydown', event => {
    if (!started || finished) return;
    event.stopImmediatePropagation();
    if (event.key === 'Escape') { event.preventDefault(); finish(); }
  }, true);
})();
