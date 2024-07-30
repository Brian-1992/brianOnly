/* ===== WEBAPP.form.QueryCombo 類別 ==============
    ‧設定limit屬性：限制顯示筆數

    ===↓繼承自WEBAPP.form.RemoteCombo↓===
       ‧設定insertEmptyRow屬性：新增一空白選項
       ‧設定queryUrl屬性自動創建store
       ‧設定queryParam屬性集合，提供查詢參數給後端程式
       ‧設定storeAutoLoad屬性，自動載入資料
==================================================== */
Ext.define('WEBAPP.form.QueryCombo', {
    extend: 'WEBAPP.form.RemoteCombo',
    alias: 'widget.querycombo',
    constructor: function (config) {
        this.callParent(arguments);
    },
    minChars: 0,
    initComponent: function (args) {
        this.callParent(args);
        this.on('beforequery', function () {
            delete this.lastQuery;
            if (Ext.isFunction(this.getDefaultParams))
                Ext.apply(this.getStore().proxy.extraParams, this.getDefaultParams());
            Ext.apply(this.getStore().proxy.extraParams, { limit: this.limit == null ? 20 : this.limit, p0: this.getValue() });
        }, this);
    }
});
