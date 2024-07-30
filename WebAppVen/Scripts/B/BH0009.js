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

    debugger

    //取得網址 http://localhost:54835/Form/Show/B/BH0009?AC-24-D1-AE-FC-C7-A4-1E-D3-AD-61-47-79-4E-52-6D-C1-56-04-57-25-F4-AD-17-F3-95-A8-8C-8D-D7-BE-8A-77-4A-B2-7A-44-4A-C6-00
    UrlString_original = location.href;

    //分割字串把分割後的字串放進陣列中
    UrlString_before = UrlString_original.split('?');
    //此時UrlString_before裡的內容為：
    //UrlString_before[0] = 'localhost:54835/Form/Index/BH0009'
    //UrlString_before[1] = 'AC-24-D1-AE-FC-C7-A4-1E-D3-AD-61-47-79-4E-52-6D-C1-56-04-57-25-F4-AD-17-F3-95-A8-8C-8D-D7-BE-8A-77-4A-B2-7A-44-4A-C6-00'
    var temp = UrlString_before[1].split('&');

    Ext.Ajax.request({
        url: '/api/BH0009/Do_DeCode',
        method: reqVal_p,
        params:
        {
            data: temp[0]
        },
        success: function (response) {
            UrlString_after = response.responseText.replace(new RegExp('"', 'g'), '');
            debugger
            //把參數部分的資料分割
            UrlString_after = UrlString_after.split('&');
            //此時UrlString_after裡的內容為：
            //UrlString_after[0] = 'po_no=INV010712270196'
            //UrlString_after[1] = 'agen_no=826'

            //每組參數再分割
            var po_no = UrlString_after[0].split('=')[1];
            //此時po_no裡的內容為：
            //po_no[0] = 'po_no'，po_no[1] = 'INV010712270196'

            var agen_no = UrlString_after[1].split('=')[1];
            //此時agen_no裡的內容為：
            //agen_no[0] = 'agen_no'，agen_no[1] = '826'

            var invoice_batno = UrlString_after[2].split('=')[1];
            //此時invoice_batno裡的內容為：
            //invoice_batno[0] = 'invoice_batno'，invoice_batno[1] = '26'

            Ext.Ajax.request({
                url: '/api/BH0009/Update',
                method: reqVal_p,
                params:
                {
                    po_no: po_no,
                    agen_no: agen_no,
                    invoice_batno: invoice_batno                    
                },
                success: function (response) {

                    document.write('您的回覆內容如下:<br>');
                    document.write('廠商編號:' + agen_no + '<br>');                 
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
