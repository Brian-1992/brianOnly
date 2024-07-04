Ext.define('WEBAPP.data.Util', {
    singleton: true,

    ajaxErrorProcess: function (proxy, response, operation, eOpts) {
        var msg = Ext.JSON.decode(response.responseText).Message;

        /*
        var msg, rawData = proxy.reader.rawData;

        if (rawData && rawData.msg) {
            msg = proxy.reader.rawData.msg;
        } else if (operation.error) {
            msg = Ext.isString(operation.error) ? operation.error : operation.error.statusText;

        */

        Ext.Msg.alert("錯誤", msg);
    }
});