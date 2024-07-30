/**
 * 單位下拉選單
 * 
 * 使用範例：
 *
 *     @example
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             { xtype: 'deptcombo' }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 * 
 */
Ext.define('MMIS.form.field.DeptComboBox', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.deptcombo',

    requires: ['MMIS.data.ComboStore'],

    name: 'DEPT',

    fieldLabel: '單位',

    displayField: 'TEXT',

    valueField: 'VALUE',
    
    matchFieldWidth: false,

    listConfig: { width: 250 },

    initComponent: function () {
        var me = this;

        me.store = Ext.create('MMIS.data.ComboStore', {
            taskFlow: 'TraDSDeptComboGet'
        }),

        me.callParent(arguments);
    }
});