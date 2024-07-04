/**
 * 日期區間欄位
 * 
 * 使用範例：
 *
 *     @example
 *     var field = Ext.create('MMIS.form.field.DateRange');
 *     
 *     Ext.create('Ext.Viewport',{
 *         layout: 'hbox',
 *         items: [field],
 *         renderTo: Ext.getBody()
 *     });
 * 
 *
 * 使用xtype:
 *
 *     @example
 *     Ext.create('Ext.form.Panel',{
 *         border: 0,
 *         items: [
 *            {
 *               xtype: 'daterangefield',
 *               startDateFieldName: "d1",
 *               endDateFieldName: "d2"
 *            },
 *            {
 *               xtype: 'daterangefield',
 *               startDateFieldName: "d3",
 *               endDateFieldName: "d4"
 *            }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 *
 * 
 */
Ext.define('MMIS.form.field.DateRange', {
    extend: 'Ext.form.FieldContainer',
    alias: 'widget.daterangefield',

    requires: ['MMIS.form.field.Month'],

    layout: 'hbox',

    fieldLabel: '日期區間',

    /**
     * @cfg {String} startDateFieldName
     * 開始日期欄位名稱
     */
    startDateFieldName: "startDate",

    /**
     * @cfg {String} endDateFieldName
     * 結束日期欄位名稱
     */
    endDateFieldName: "endDate",

    /**
     * @cfg {String} middleSymbol
     * 開始與欄位結束日期欄位間間隔符號
     */
    middleSymbol: '~',

    /**
     * @cfg {Object} dateFieldConfig
     * 用於開始與結束日期欄位設定，見Ext.form.field.Date
     */
    dateFieldConfig: null,

    /**
     * @cfg {Object} startDateFieldConfig
     * 用於開始日期欄位設定，見Ext.form.field.Date
     */
    startDateFieldConfig: null,

    /**
     * @cfg {Object} endDateFieldConfig
     * 用於結束日期欄位設定，見Ext.form.field.Date
     */
    endDateFieldConfig: null,

    /**
     * 取得開始日期欄位
     * @return {Ext.form.field.Date}     
     */
    getStartDateField: function () {
        return this.startDateField;
    },

    /**
     * 取得結束日期欄位
     * @return {Ext.form.field.Date}     
     */
    getEndDateField: function () {
        return this.endDateField;
    },

    /**
     * @cfg {Boolean} onlySelectMonth
     * 使用年月選擇器
     */
    monthYearSelect: false,

    /**
     * @cfg {DateTime} defaultStartDate
     * 預設起始時間
     */
    startDate: null,

    /**
    * @cfg {DateTime} defaultEndDate
    * 預設結束時間
    */
    endDate: null,

    initComponent: function () {        
        var me = this,
            startDateField =
                me.generateDateField(me.startDateFieldName, me.startDateFieldConfig, me.startDate),
            endDateField =
                me.generateDateField(me.endDateFieldName, me.endDateFieldConfig, me.endDate);

        /**
         * @event change
         * Fires when any fields that in the fieldContainer change value.
         *
         * @param {MMIS.form.field.DateRange} this
         * @param {Ext.form.field.Field} field
         * @param {Object} newValue The new value
         * @param {Object} oldValue The original value
         */
        me.addEvents('change');

        me.createFieldInteraction(startDateField, endDateField);

        Ext.apply(me, {
            startDateField: startDateField,
            endDateField: endDateField,
            items: [
                startDateField,
                {
                    xtype: 'component',
                    autoEl: { tag: 'span', html: me.middleSymbol }
                },
                endDateField
            ]
        });

        me.callParent(arguments);
    },

    generateDateField: function (name, fieldConfig, value) {
        var me = this,
            config = Ext.apply({
                name: name,
                flex: 1,
                value: value
            }, me.dateFieldConfig, fieldConfig),
            fieldType = me.monthYearSelect ? 'monthfield' : 'datefield',
            field = Ext.widget(fieldType, config);

        field.on('change', function (field, newValue, oldValue) {
            me.fireEvent('change', me, field, newValue, oldValue);
        });

        return field;
    },

    createFieldInteraction: function (startDateField, endDateField) {
        startDateField.setMaxValue(endDateField.getValue());
        endDateField.setMinValue(startDateField.getValue());

        startDateField.addListener('change', function (field) {
            endDateField.setMinValue(field.getValue());
            endDateField.validate();
        });

        endDateField.addListener('change', function (field) {
            startDateField.setMaxValue(field.getValue());
            startDateField.validate();
        });
    }
});