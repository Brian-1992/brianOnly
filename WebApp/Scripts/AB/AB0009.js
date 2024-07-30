Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.Ajax.request({
        url: '/api/AB0009/CreateMrDoc',
        method: reqVal_p,
        params: {
            //DOCNO: docno,
            //FLOWID: '0602'
        },
        //async: true,
        success: function (response) {
            var data = Ext.decode(response.responseText);
            if (data.success) {
                msglabel('執行成功');
            }
            else
                //Ext.MessageBox.alert('錯誤', data.msg);
                msglabel(data.msg);
        },
        failure: function (response) {
            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
        }
    });
});