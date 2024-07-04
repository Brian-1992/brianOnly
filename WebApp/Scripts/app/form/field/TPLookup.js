/**
 * 列車財產下拉選單
 * 
 * 使用範例：
 *
 *     @example
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             {
 *                 fieldLabel: '量測類型',
 *                 name: 'MEA_KIND',
 *                 xtype: 'tplookup',
 *                 lookupTableName: 'TP_MEASURE',
 *                 lookupColName: 'MEA_KIND'
 *             }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 * 
 */
Ext.define('MMIS.form.field.TPLookup', {
    extend: 'Ext.form.field.ComboBox',
    requires: ['MMIS.data.TPLookupStore'],
    alias: 'widget.tplookup',

    /**
     * @cfg {String} lookupTableName
     * 對應至 TP_LOOKUP 資料表 lookupTableName 欄位值
     */
    lookupTableName: null,

    /**
     * @cfg {String} lookupColName
     * 對應至 TP_LOOKUP 資料表 lookupColName 欄位值
     */
    lookupColName: null,

    textType: 0, //0:TEXT(VALUE) 1:TEXT

    displayField: 'TEXT',

    valueField: 'VALUE',

    queryMode: 'local',

    initComponent: function () {
        var me = this;
        me.store = me.generateStore();
        me.callParent(arguments);
    },

    _beforeInitComponent: function () {
        this.store = this._generateStore();
    },

    generateStore: function () {
        var me = this;
        return Ext.create('MMIS.data.TPLookupStore', {
            lookupTableName: me.lookupTableName,
            lookupColName: me.lookupColName,
            textType: me.textType
        });
    }
});