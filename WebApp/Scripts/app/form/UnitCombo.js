/** */
Ext.define('WEBAPP.form.UnitCombo', {
    extend: 'WEBAPP.form.QueryCombo',
    alias: 'widget.mmcodecombo',
    constructor: function (config) {
        var me = this;
        config = config || {};

        Ext.apply(config, me.getDefaults());
        Ext.apply(me, config);
        me.callParent(arguments);
    },

    getDefaults: function () {
        return {
            displayField: 'PACK_UNIT',
            valueField: 'PACK_UNIT',
            requiredFields: ['PACK_QTY'],
            //storeAutoLoad: true,
            //queryUrl: '/api/AA0041/GetMMCodeCombo',
            tpl: new Ext.XTemplate(
                '<tpl for=".">',
                '<tpl if="VALUE==\'\'">',
                '<div class="x-boundlist-item" style="height:auto;">{PACK_UNIT}&nbsp;</div>',
                '<tpl else>',
                '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                '<span style="color:red">{PACK_UNIT}</span><br/>&nbsp;<span style="color:blue">{PACK_QTY}</span></div>',
                '</tpl></tpl>', {
                    formatText: function (text) {
                        return Ext.util.Format.htmlEncode(text);
                    }
                })
        };
    }
});