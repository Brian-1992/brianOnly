/**
 * 財產單位下拉選單
 */
Ext.define('MMIS.form.field.PrDeptComboBox', {
    extend: 'MMIS.form.field.XCombo',
    alias: 'widget.prdeptcombo',

    fieldLabel: '財產單位',

    taskFlow: 'TraPrDeptComboGet',

    matchFieldWidth: false,

    listConfig: { width: 250 }

});