/**
 * 存料單位選擇欄位
 *
 * 使用範例：
 *
 *     @example
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             { 
 *                 xtype: 'mminvdeptselector',
 *                 windowWidth: 500,
 *                 windowHeight: 200
 *             }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 */
Ext.define('MMIS.form.field.MMInvDeptSelector', {
    extend: 'MMIS.form.field.ItemSelector',
    alias: 'widget.mminvdeptselector',

    fieldLabel: '存料單位',

    xtype: 'triggeritemselector',

    title: '請選擇存料單位',

    taskFlow: 'TraMMInvDeptComboGet',

    editable: false
});