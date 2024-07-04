/** */
Ext.define('WEBAPP.form.ParamCombo', {
    extend: 'WEBAPP.form.RemoteCombo',

    constructor: function (config) {
        var me = this;
        config = config || {};
        config.storeAutoLoad = true;
        config.queryUrl = '/api/GB0006/GetCombo';
        
        Ext.apply(me, config);
        me.callParent(arguments);
    }
});
