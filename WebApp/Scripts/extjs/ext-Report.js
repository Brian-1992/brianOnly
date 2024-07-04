/**
 * Report class
 * By Chilun
 * 14 Nov 2012
 */

// 2014-11-25: Chilun, 與ext-pickup.js全域變數同名問題之解決

var winR = null;
var reportupT = null;
var reportupF = null;

var reportupWin = [];

//顯示Popup Form
function showReport(viewport, reportup, title, name) {
    reportupF = reportup;
    if (reportupF != reportupT) {
        //檢查formWin存在
        var bExist = false;
        if (reportupWin.length > 0) {
            var i;
            for (i = 0; i < reportupWin.length; i++) {
                if (reportupWin[i].form == reportup) {
                    winR = reportupWin[i].win;
                    bExist = true;
                    break;
                }
            }
        }
        if (!bExist) {
            winR = null;
            var winActWidth = viewport.width - 10;
            var winActHeight = viewport.height - 10;
            winR = Ext.widget('window', {
                title: title,
                modal: true,
                layout: 'fit',
                autoScroll: true,
                closeAction: 'distroy',
                constrain: true,
                resizable: true,
                width: winActWidth, maxWidth: winActWidth,
                height: winActHeight, maxHeight: winActHeight,
                items: [{
                    //id: 'iframeReport',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    html: '<iframe src="" id="' + name + '" name="' + name + '" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
                    ,buttons: [{
                        text: '關閉',
                        handler: function () {
                            hideReport();
                        }
                    }]
                }],
                listeners: {
                    beforeclose: {
                        fn: function () {
                            hideReport();
                        }
                    },
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
            reportupWin.push({
                'form': reportupF,
                'win': winR
            });
        }

        reportupT = reportup;
    }
    if (winR.hidden) {
        winR.setTitle(title);
        winR.show();
    }
    //return winR;
}

//顯示Popup Form
function showQueryReport(viewport, reportup, title, name) {
    reportupF = reportup;
    if (reportupF != reportupT) {
        //檢查formWin存在
        var bExist = false;
        if (reportupWin.length > 0) {
            var i;
            for (i = 0; i < reportupWin.length; i++) {
                if (reportupWin[i].form == reportup) {
                    winR = reportupWin[i].win;
                    bExist = true;
                    break;
                }
            }
        }
        if (!bExist) {
            winR = null;
            var winActWidth = viewport.width - 10;
            var winActHeight = viewport.height - 10;
            winR = Ext.widget('window', {
                title: title,
                modal: true,
                //layout: 'fit',
                autoScroll: true,
                closeAction: 'distroy',
                constrain: true,
                resizable: true,
                layout: 'border',
                resizable: true,
                width: winActWidth, maxWidth: winActWidth,
                height: winActHeight, maxHeight: winActHeight,
                items: [{
                    region: 'north',
                    collapsible: false,
                    border: false,
                    layout: 'fit',
                    items: [reportup]
                }, {
                    //id: 'iframeReport',
                    region: 'center',
                    collapsible: false,
                    title: '',
                    border: false,
                    layout: {
                        type: 'fit',
                        padding: 5,
                        align: 'stretch'
                    },
                    html: '<iframe src="" id="' + name + '" name="' + name + '" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
                    , buttons: [{
                        text: '關閉',
                        handler: function () {
                            hideReport();
                        }
                    }]
                }],
                listeners: {
                    beforeclose: {
                        fn: function () {
                            hideReport();
                        }
                    },
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
            reportupWin.push({
                'form': reportupF,
                'win': winR
            });
        }

        reportupT = reportup;
    }
    if (winR.hidden) {
        winR.setTitle(title);
        winR.show();
        //var winActWidth = viewport.width - 10;
        //var winActHeight = viewport.height - 10;
        //var winBefWidth = winR.getWidth();
        //var winBefHeight = winR.getHeight();
        //if ((winActHeight != winBefHeight) || (winActWidth != winBefWidth)) {
        //    winR.setHeight(winActHeight);
        //    winR.setWidth(winActWidth);
        //}
    }
    //return winR;
}

//隱藏Popup Form
function hideReport() {
    if (!winR.hidden) {
        if (reportupF != reportupT) {
            winR.close();
        }
        else {
            winR.hide();
        }
    }
}
//設定Popup Title
function setReportTitle(t) {
    try {
        winR.setTitle(t);
    } catch (e) {

    }
}
//設定Popup Form Focus
function setReportFocus(f) {
    try {
        winR.defaultFocus = f;
    } catch (e) {

    }
}