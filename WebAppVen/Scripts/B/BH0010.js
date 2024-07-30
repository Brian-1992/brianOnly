Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

debugger

Ext.getUrlParam = function (param) {
    var params = Ext.urlDecode(location.search.substring(1));
    return param ? params[param] : params;
};

Ext.onReady(function () {

    var UrlString_original = '';
    var UrlString_before = '';
    var UrlString_after = '';
    var reqVal_p = 'POST';

    debugger

             
    //取得網址 http://localhost:54835/Form/Show/B/BH0010?F8-AF-EC-3A-56-E5-75-E7-F2-4A-D8-01-57-E5-48-A6-D2-AE-5A-3F-DA-CE-18-C3
    UrlString_original = location.href;

    //分割字串把分割後的字串放進陣列中
    UrlString_before = UrlString_original.split('?');
    //此時UrlString_before裡的內容為：
    //UrlString_before[0] = 'localhost:54835/Form/Show/B/BH0010'
    //UrlString_before[1] = 'F8-AF-EC-3A-56-E5-75-E7-F2-4A-D8-01-57-E5-48-A6-D2-AE-5A-3F-DA-CE-18-C3'
    var temp = UrlString_before[1].split('&');

    Ext.Ajax.request({
        url: '/api/BH0010/Do_DeCode',
        method: reqVal_p,
        params:
        {
            data: temp
        },
        success: function (response) {
            UrlString_after = response.responseText.replace(new RegExp('"', 'g'), '');
            debugger
            
            //此時UrlString_after裡的內容為：
            //UrlString_after[0] = 'CRDOCNO=EMG0000019'
           
            //參數再分割
            var crdocno = UrlString_after.split('=')[1];
            //此時crdocno裡的內容為：
            //crdocno[0]='CRDOCNO'，crdocno[1]='EMG0000019'

            Ext.Ajax.request({
                url: '/api/BH0010/Update',
                method: reqVal_p,
                params:
                {
                    crdocno: crdocno            
                },
                success: function (response) {
                    document.write('您的回覆內容如下:<br>');
                    document.write('流水號:' + crdocno + '<br>');
                    document.write('感謝您,已收到您的回覆函<br>');
                },
                failure: function (response, options) {
                    document.write('fail');
                }
            });
        },
        failure: function (response, options) {
        }
    });


});
