mergeInto(LibraryManager.library, {
  GetBrowserId: function() {
    var id = localStorage.getItem('unityPlayerId');
    if (!id) {
      id = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
      });
      localStorage.setItem('unityPlayerId', id);
    }
    var bufferSize = lengthBytesUTF8(id) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(id, buffer, bufferSize);
    return buffer;
  }
});