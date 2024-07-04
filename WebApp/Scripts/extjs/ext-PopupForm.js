// PopupForm class
// By Chilun
// 29 Aug 2013
// 2014-12-02 Chilun, 改良Popup Form


var winF = null;
var formT = null;
var formF = null;

var formWin = [];

//顯示Popup Form
function showPopForm(viewport, form, title, width, height) {
    formF = form;
    if (formF != formT) {
        //檢查formWin存在
        var bExist = false;
        if (formWin.length > 0) {
            var i;
            for (i = 0; i < formWin.length; i++) {
                if (formWin[i].form==form) {
                    winF = formWin[i].win;
                    bExist = true;
                    break;
                }
            }
        }
        if (!bExist) {
            winF = null;
            //var winActWidth = width;
            //var winActHeight = height;
            var winActWidth = viewport.width - 10;
            var winActHeight = viewport.height - 10;
            winF = Ext.widget('window', {
                title: title,
                layout: 'fit',
                autoScroll: false,
                constrain: true,
                resizable: true,
                modal: true,
                width: winActWidth, maxWidth: winActWidth,
                height: winActHeight, maxHeight: winActHeight,
                items: form,
                closeAction: 'hide',
                listeners: {
                    move: function (xwin, x, y, eOpts) {
                        xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                        xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                    },
                    resize: function (xwin, width, height) {
                        winActWidth = width;
                        winActHeight = height;
                    }
                }
            });
            formWin.push({
                'form': formF,
                'win': winF
            });
        }

        formT = form;
    }
    if (winF.hidden) {
        winF.show();
        var winActWidth = viewport.width - 10;
        var winActHeight = viewport.height - 10;
        var winBefWidth = winF.getWidth();
        var winBefHeight = winF.getHeight();
        if ((winActHeight != winBefHeight) || (winActWidth != winBefWidth)) {
            winF.setHeight(winActHeight);
            winF.setWidth(winActWidth);
        }
    }
    //return winF;
}
//隱藏Popup Form
function hidePopForm() {
    // Modified by Roy, 2015-07-30, 隱藏視窗時檢查視窗是否存在。
    if (winF && !winF.hidden) {
        if (formF != formT)
        {
            winF.close();
        }
        else {
            winF.hide();
        }
    }
}
//設定Popup Title
function setPopFormTitle(t) {
    try {
        winF.setTitle(t);
    } catch (e) {

    }
}
//設定Popup Form Focus
function setPopFormFocus(f) {
    try {
        winF.defaultFocus = f;
    } catch (e) {

    }
}


// 2014-12-02 Chilun, 改良Popup Form
var formArray = [];
var winForm = null;
var modelForm = null;

//檢查formArray有否存在
function checkFormExist() {
    var bExist = false;
    if (formArray.length > 0) {
        for (i = 0; i < formArray.length; i++) {
            if (formArray[i].model == modelForm) {
                winForm = formArray[i].win;
                bExist = true;
                break;
            }
        }
    }
    return bExist;
}
//設定Popup Form
function setPopupForm(viewport, form, title) {
    winForm = null;
    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;
    winForm = Ext.widget('window', {
        title: title,
        layout: 'fit',
        autoScroll: false,
        constrain: true,
        resizable: true,
        modal: true,
        width: winActWidth, maxWidth: winActWidth,
        height: winActHeight, maxHeight: winActHeight,
        items: form,
        closeAction: 'hide',
        listeners: {
            move: function (xwin, x, y, eOpts) {
                xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
            },
            resize: function (xwin, width, height) {
                winActWidth = width;
                winActHeight = height;
            }
        }
    });
    formArray.push({
        'model': modelForm,
        'win': winForm
    });
}
//顯示Popup Form
function showPopupForm(viewport,title) {
    if (winForm.hidden) {
        winForm.show();
        setPopupFormTitle(title);
    }
}
//隱藏Popup Form
function hidePopupForm() {
    // Modified by Roy, 2015-07-30, 隱藏視窗時檢查視窗是否存在。
    if (winForm && !winForm.hidden) {
        if (winForm != formT) {
            winForm.close();
        }
        else {
            winForm.hide();
        }
    }
}
//設定Popup Title
function setPopupFormTitle(t) {
    try {
        winForm.setTitle(t);
    } catch (e) {
    }
}
//設定Popup Form Focus
function setPopupFormFocus(f) {
    try {
        winForm.defaultFocus = f;
    } catch (e) {

    }
}
