/** */
Ext.define('WEBAPP.form.AgenNoCombo_2', {
    extend: 'WEBAPP.form.QueryCombo',
    alias: 'widget.agennocombo',
    constructor: function (config) {
        var me = this;
        config = config || {};

        Ext.apply(config, me.getDefaults());
        Ext.apply(me, config);
        me.callParent(arguments);
    },

    getDefaults: function () {
        return {
            displayField: 'AGEN_NO',
            valueField: 'AGEN_NO',
            requiredFields: ['AGEN_NAMEC'],
            //storeAutoLoad: true,
            tpl: new Ext.XTemplate(
                '<tpl for=".">',
                '<tpl if="VALUE==\'\'">',
                '<div class="x-boundlist-item" style="height:auto;">{AGEN_NO}&nbsp;</div>',
                '<tpl else>',
                '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                '<span style="color:red">{AGEN_NO}</span><br/>&nbsp;<span style="color:blue">(中) {AGEN_NAMEC}</span><br/></div>',
                '</tpl></tpl>', {
                    formatText: function (text) {
                        return Ext.util.Format.htmlEncode(text);
                    }
                })
        };
    }
});
