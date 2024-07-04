function showPopFormF(strUrl, strTitle, strParam, viewport) {
    if (!winS) {
        strUrl = strUrl + '?strParam=' + strParam;
        var popform = Ext.create('Ext.form.Panel', {
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
            ,
            buttons: [{
                text: '關閉',
                handler: function () {
                    this.up('form').getForm().reset();
                    this.up('window').destroy();
                }
            }]
        });

        var winActWidth = 1000;
        var winActHeight = 1000;
        var winS = Ext.widget('window', {
            title: strTitle,
            id:'winS',
            modal: true,
            layout: 'fit',
            autoScroll: true,
            closeAction: 'destroy',
            constrain: true,
            resizable: true,
            width: winActWidth,// maxWidth: winActWidth,
            height: winActHeight, //maxHeight: winActHeight,
            items: popform,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : 1000);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : 100);
                },
                resize: function (xwin, width, height) {
                    winActWidth = 1000;
                    winActHeight = 1000;
                }
            }
        });
    }
    winS.show();
}