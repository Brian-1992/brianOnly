Ext.define('MMIS.data.TreeStore', {
    extend: 'Ext.data.TreeStore',

    requires: ['MMIS.data.Util'],

    constructor: function (config) {
        var me = this;
        me.callParent(arguments);
        me.proxy.on('exception', function (proxy, response, operation) {
            MMIS.data.Util.ajaxErrorProcess(proxy, response, operation);
        });
    }
});
