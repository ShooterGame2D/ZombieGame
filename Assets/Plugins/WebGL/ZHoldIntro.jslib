mergeInto(LibraryManager.library, {
  ZHold_BeginIntro: function(receiver, volume) {
    var target = UTF8ToString(receiver);
    if (typeof window.startZHoldIntro === 'function') window.startZHoldIntro(target, volume);
    else SendMessage(target, 'CompleteIntro');
  }
});
