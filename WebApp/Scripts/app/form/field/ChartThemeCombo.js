/**
 * 圖表樣式下拉選單
 */
Ext.define('MMIS.form.field.ChartThemeCombo', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.charttheme',

    store: Ext.create('Ext.data.Store', {
        fields: ['text', 'value'],
        data: [
            { text: 'Base', value: 'Base' },
            { text: 'Green', value: 'Green' },
            { text: 'Sky', value: 'Sky' },
            { text: 'Red', value: 'Red' },
            { text: 'Purple', value: 'Purple' },
            { text: 'Blue', value: 'Blue' },
            { text: 'Yellow', value: 'Yellow' },
            { text: 'Category1', value: 'Category1' },
            { text: 'Category2', value: 'Category2' },
            { text: 'Category3', value: 'Category3' },
            { text: 'Category4', value: 'Category4' },
            { text: 'Category5', value: 'Category5' },
            { text: 'Category6', value: 'Category6' }
        ]
    }),
    valueField: 'value'
});