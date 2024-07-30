/**
 * 存料單位樹狀選單
 */
Ext.define('MMIS.form.field.InvDeptTreePicker', {
    extend: 'MMIS.form.field.TreePicker',
    alias: 'widget.invdepttreepicker',

    fieldLabel: '存料單位',

    url: '../../../api/DSTree/GetInvDeptNodes',

    width: 300
});