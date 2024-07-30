/** */
Ext.define('WEBAPP.form.MMCodeCombo', {
    extend: 'WEBAPP.form.QueryCombo',
    alias: 'widget.mmcodecombo',
    constructor: function (config) {
        var me = this;
        config = config || {};
        
        Ext.apply(config, me.getDefaults());
        Ext.apply(me, config);
        me.callParent(arguments);
    },

    getDefaults: function ()
    {
        return {
            displayField: 'MMCODE',
            valueField: 'MMCODE',
            requiredFields: ['MMNAME_C', 'MMNAME_E'],
            //storeAutoLoad: true,
            //queryUrl: '/api/AA0041/GetMMCodeCombo',
            tpl: new Ext.XTemplate(
                '<tpl for=".">',
                '<tpl if="VALUE==\'\'">',
                '<div class="x-boundlist-item" style="height:auto;">{MMCODE}&nbsp;</div>',
                '<tpl else>',
                '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                '<span style="color:red">{MMCODE}</span><br/>&nbsp;<span style="color:blue">(英) {MMNAME_E}</span><br/>&nbsp;(中) {MMNAME_C}</div>',
                '</tpl></tpl>', {
                    formatText: function (text) {
                        return Ext.util.Format.htmlEncode(text);
                    }
                })
        };
    }
});
