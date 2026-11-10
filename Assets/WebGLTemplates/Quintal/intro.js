(() => {
  'use strict';
  if (!document.querySelector('#intro')) {
    const section=document.createElement('section'); section.id='intro'; section.hidden=true; section.style.cssText='position:fixed;inset:0;background:black;z-index:99999';
    section.innerHTML='<video id="intro-video" playsinline style="width:100%;height:100%;object-fit:contain"></video><button id="intro-sound" style="position:absolute;left:24px;top:24px;padding:16px">Reproduzir / ativar som</button><button id="intro-skip" style="position:absolute;right:24px;top:24px;padding:16px">Pular introdução</button>'; document.body.appendChild(section);
  }
  const overlay = document.querySelector('#intro');
  const video = document.querySelector('#intro-video');
  const sound = document.querySelector('#intro-sound');
  const skip = document.querySelector('#intro-skip');
  const canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
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
      else { console.warn('Z-Hold intro:',error); sound.hidden=false; sound.textContent='Reproduzir introdução'; }
    });
  }
  function start() {
    if (!instance || !request || started) return;
    started = true; finished=false;
    video.src=new URL('StreamingAssets/Cinematics/Z-Hold-Intro.mp4',document.baseURI).href; video.load();
    canvas.inert = true;
    canvas.blur();
    overlay.hidden = false;
    video.volume = Math.max(0, Math.min(1, request.volume));
    if (video.error) { finish(); return; }
    skip.focus();
    attemptPlay();
  }
  window.startZHoldIntro = (receiver, volume) => { request = { receiver, volume }; started=false; finished=false; start(); };
  window.zholdIntroReady = game => { instance = game; start(); };
  sound.addEventListener('click', () => attemptPlay(false));
  skip.addEventListener('click', finish);
  video.addEventListener('ended', finish);
  video.addEventListener('error', () => {
    if(!started||finished)return;
    if(!video.src.endsWith('.webm')) {video.src=new URL('StreamingAssets/Cinematics/Z-Hold-Intro.webm',document.baseURI).href;attemptPlay(true);}
    else {sound.hidden=false;sound.textContent='Vídeo indisponível — tente novamente';}
  });
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
