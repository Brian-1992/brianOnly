/** */
Ext.define('WEBAPP.form.UrInidCombo', {
    extend: 'WEBAPP.form.QueryCombo',
    alias: 'widget.urinidcombo',
    constructor: function (config) {
        var me = this;
        config = config || {};

        Ext.apply(config, me.getDefaults());
        Ext.apply(me, config);
        me.callParent(arguments);
    },

    getDefaults: function () {
        return {
            displayField: 'INID',
            valueField: 'INID',
            requiredFields: ['INID_NAME'],
            //storeAutoLoad: true,
            tpl: new Ext.XTemplate(
                '<tpl for=".">',
                '<tpl if="VALUE==\'\'">',
                '<div class="x-boundlist-item" style="height:auto;">{INID}&nbsp;</div>',
                '<tpl else>',
                '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                '<span style="color:red">{INID}</span><br/>&nbsp;<span style="color:blue">{INID_NAME}</span></div>',
                '</tpl></tpl>', {
                    formatText: function (text) {
                        return Ext.util.Format.htmlEncode(text);
                    }
                })
        };
    }
});
