
Ext.onReady(function () {
    var viewport = Ext.create('Ext.Viewport', {
        layout: {
            type: 'border',
            padding: 50
        },

        items: [{
            region: 'center',
            layout: 'fit',
            title: '',
            collapsible: false,
            split: false,
            border: false,
            //html: '<div style="text-align:center; background-image:url(\'../../../Images/AccessDeniedBG.jpg\');background-size:contain; height:500px; width:1000px;"><br><br><br><br><br><br><br><br><br><br><br><br><br><br><img src="../../../Images/AccessDenied.png" style="width:75px; height:100px;" /><div style="color:#fff;">權限不足(群組功能關聯)</div></div>'
            //html: '<div style="text-align:center;background-size:contain; height:500px; width:1000px;"></div>'
            html: '<div class=\"modal-dialog\" role=\"document\"><div class=\"modal-content\"><div class=\"modal-body\" style=\"color:darkblue;text-align:center;\">您的連線已逾期或未授權，請重新登入。</div></div></div>'
        }]
    });

});