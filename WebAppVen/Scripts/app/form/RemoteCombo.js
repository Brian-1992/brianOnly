/* ===== WEBAPP.form.RemoteCombo 類別 ==============
    ‧設定insertEmptyRow屬性：新增一空白選項
    ‧設定queryUrl屬性自動創建store
    ‧設定queryParam屬性集合，提供查詢參數給後端程式
    ‧設定storeAutoLoad屬性，自動載入資料
==================================================== */
Ext.define('WEBAPP.form.RemoteCombo', {
    extend: 'Ext.form.ComboBox',
    requires: ['WEBAPP.data.RemoteStore'],
    alias: 'widget.remotecombo',
    constructor: function (config) {
        var me = this;
        config = config || {};
        Ext.apply(me, Ext.applyIf(config, me.getDefaultConfig(config)));
        var storeFields = (this.displayField == this.valueField) ? [this.valueField] : [this.displayField, this.valueField];
        storeFields = this.requiredFields ? Ext.Array.merge(storeFields, config.requiredFields) : storeFields;
        storeFields = this.extraFields ? Ext.Array.merge(storeFields, config.extraFields) : storeFields;
        config.store = Ext.create('WEBAPP.data.RemoteStore', {
            autoLoad: config.storeAutoLoad,
            insertEmptyRow: config.insertEmptyRow,
            fields: storeFields,
            queryParam: config.queryParam,
            proxy: { url: config.queryUrl }
        });
        //Ext.apply(me, config);
        me.callParent(arguments);
    },

    getDefaultConfig: function (config) {
        return {
            displayField: 'TEXT',
            valueField: 'VALUE'
        }
    }
});
