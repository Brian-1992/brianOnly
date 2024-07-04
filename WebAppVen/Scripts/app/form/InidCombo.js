/** */
Ext.define('WEBAPP.form.InidCombo', {
    extend: 'WEBAPP.form.RemoteCombo',

    constructor: function (config) {
        var me = this;
        config = config || {};
        config.queryUrl = '/api/AA0041/GetCombo';

        //Ext.apply(me, Ext.applyIf(config, me.getDefaultConfig(config)));
        Ext.apply(me, config);
        me.callParent(arguments);
    }
});
