Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.getUrlParam = function (param) {
    var params = Ext.urlDecode(location.search.substring(1));
    return param ? params[param] : params;
};

Ext.onReady(function () {
    var UrlString_original = '';
    var UrlString_before = '';
    var UrlString_after = '';
    var reqVal_p = 'POST';

    //原網址 取得網址 http://localhost:54835/Form/Show/B/BH0007?AGEN_NO=013&MAIL_NO=1080920150841
    //取得網址 http://localhost:54835/Form/Show/B/BH0007?C0-09-F8-06-E5-35-3A-D1-13-16-6F-9F-18-2D-15-91-33-43-88-91-C2-B4-41-AA-6F-3A-35-B6-EC-DE-44-D8-CE-F1-7F-AB-F7-42-30-4C
    UrlString_original = location.href;

    //分割字串把分割後的字串放進陣列中
    UrlString_before = UrlString_original.split('?');
    //此時UrlString_before裡的內容為：
    //UrlString_before[0] = 'localhost:54835/Form/Index/BH0007'
    //UrlString_before[1] = 'C0-09-F8-06-E5-35-3A-D1-13-16-6F-9F-18-2D-15-91-33-43-88-91-C2-B4-41-AA-6F-3A-35-B6-EC-DE-44-D8-CE-F1-7F-AB-F7-42-30-4C'

    Ext.Ajax.request({
        url: '/api/BH0007/Do_DeCode',
        method: reqVal_p,
        params:
        {
            data: UrlString_before[1]
        },
        success: function (response) {
            UrlString_after = response.responseText.replace(new RegExp('"', 'g'), ''); //去除雙引號
            debugger
            //把參數部分的資料分割
            UrlString_after = UrlString_after.split('&');
            //此時UrlString_after裡的內容為：
            //UrlString_after[0] = 'AGEN_NO=013&MAIL_NO=1080920150841'
            debugger
            //每組參數再分割
            var AGEN_NO = UrlString_after[0].split('=')[1];
            var MAIL_NO = UrlString_after[1].split('=')[1];
            //此時AGEN_NO裡的內容為：
            //AGEN_NO[0] = 'AGEN_NO'，AGEN_NO[1] = '013'
            //MAIL_NO[0] = 'MAIL_NO'，MAIL_NO[1] = '1080920150841'

            Ext.Ajax.request({
                url: '/api/BH0007/Create',
                method: reqVal_p,
                params:
                {
                    AGEN_NO: AGEN_NO,
                    MAIL_NO: MAIL_NO
                },
                success: function (response) {
                    /*document.write('您的回覆內容如下:<br>');
                    document.write('院內碼:' + MMCODE + '<br>');
                    document.write('月份:' + EXP_DATE + '<br>');
                    document.write('批號:' + LOT_NO + '<br>');
                    document.write('數量:' + EXP_QTY + '<br>');*/
                    document.write('廠商代碼:' + AGEN_NO + '<br><br>');
                    document.write('感謝您,已收到您的回覆函');
                },
                failure: function (response, options) {

                }
            });
        },
        failure: function (response, options) {
        }
    });


});
