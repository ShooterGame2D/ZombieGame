mergeInto(LibraryManager.library, {
  ZHold_BeginIntro: function(receiver, volume) {
    var target=UTF8ToString(receiver);
    var instance={SendMessage:function(name,method){SendMessage(name,method);}};
    function begin(){window.zholdIntroReady(instance);window.startZHoldIntro(target,volume);}
    if(typeof window.startZHoldIntro==='function'){begin();return;}
    var script=document.createElement('script');
    script.src=new URL('StreamingAssets/zhold-intro.js',document.baseURI).href;
    script.onload=begin;
    script.onerror=function(){console.error('Z-Hold: falha ao carregar introdução');SendMessage(target,'CompleteIntro');};
    document.head.appendChild(script);
  }
});
