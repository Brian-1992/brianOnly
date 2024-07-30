/**
 * 支援使用 taskFlow 做為資料來源的 combobox 。
 *
 * 使用範例：
 *
 *     @example
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             { 
 *                 xtype: 'xcombo',
 *                 fieldLabel: '檢修別',
 *                 name: 'MT_LEVEL',
 *                 taskFlow: 'TraTPMtLevelGetLOOKUP'
 *             }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 */
Ext.define('MMIS.form.field.XCombo', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.xcombo',

    requires: ['MMIS.data.ComboStore'],

    /**
     * @cfg {String} taskFlow (required)
     */

    /**
     * @cfg {String} rootTable
     */

    displayField: 'TEXT',

    valueField: 'VALUE',

    insertEmptyRow: true,

    initComponent: function () {
        var me = this;

        me.store = me.store || me.createStore();

        me.callParent(arguments);
    },

    createStore: function () {
        var me = this;
        return Ext.create('MMIS.data.ComboStore', {
            taskFlow: me.taskFlow,
            rootTable: me.rootTable,
            insertEmptyRow: me.insertEmptyRow
        });
    },

    reload: function () {
        this.store.reload();
    }
});