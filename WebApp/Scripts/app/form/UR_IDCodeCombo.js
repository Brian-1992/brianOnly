/** */
Ext.define('WEBAPP.form.UR_IDCodeCombo', {
    extend: 'WEBAPP.form.QueryCombo',
    alias: 'widget.ur_idcodecombo',
    constructor: function (config) {
        var me = this;
        config = config || {};

        Ext.apply(config, me.getDefaults());
        Ext.apply(me, config);
        me.callParent(arguments); 
    },

    getDefaults: function () {
        return { 
            displayField: 'TUSER',
            valueField: 'TUSER',
            requiredFields: ['INID'],
            //storeAutoLoad: true,
            //queryUrl: '/api/AA0041/GetMMCodeCombo',
            tpl: new Ext.XTemplate(
                '<tpl for=".">',
                '<tpl if="VALUE==\'\'">',
                '<div class="x-boundlist-item" style="height:auto;">{TUSER}&nbsp;</div>',
                '<tpl else>',
                '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                '<span style="color:red">{TUSER}</span><br/>&nbsp;<span style="color:blue">{UNA}</span></div>',
                '</tpl></tpl>', {
                    formatText: function (text) {
                        return Ext.util.Format.htmlEncode(text);
                    }
                })
        };
    }
});
