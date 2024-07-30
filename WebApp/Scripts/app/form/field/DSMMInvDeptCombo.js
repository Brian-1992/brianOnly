/**
 * 存料單位下拉選單
 */
Ext.define('MMIS.form.field.DSMMInvDeptCombo', {
    extend: 'MMIS.form.field.XCombo',
    alias: 'widget.dsmminvdeptcombo',

    fieldLabel: '存料單位',

    taskFlow: 'TraMMInvDeptComboGet',

    matchFieldWidth: false,

    listConfig: { width: 200 }

});