Ext.define('MMIS.form.field.DateRangeField', {
    extend: 'Ext.form.FieldContainer',
    alias: 'widget.daterangefield',

    fieldLabel: '查詢日期',
    combineErrors: true,
    layout: 'hbox',

    config: {
        startDateField: null,
        endDateField: null,
        middleSymbol: '~'
    },

    startDateFieldName: "startDate",
    endDateFieldName: "endDate",
    startDate: null,
    endDate: null,

    startDateFieldConfig: null,
    endDateFieldConfig: null,

    startDateListener: {},
    endDateListener: {},       

    //Public methods
    setStartDate: function (date) {
        this.startDateField.setValue(date);
    },

    setEndDate: function (date) {
        this.endDateField.setValue(date);
    },

    getStartDateValue: function () {
        return this.startDateField.getValue();
    },

    getEndDateValue: function () {
        return this.endDateField.getValue();
    },

    getStartDateRawValue: function () {
        return this.startDateField.getRawValue();
    },

    getEndDateRawValue: function () {
        return this.endDateField.getRawValue();
    },
    
    initComponent: function () {
        this.beforeInitComponent();
        this.callParent(arguments);
        this.afterInitComponent();
    },

    beforeInitComponent: function () {
        this.startDateField = this.generateStartDateField();
        this.endDateField = this.generateEndDateFieldField();

        this.createFieldInteraction();

        this.items = [
            this.startDateField, {
                xtype: 'component',
                autoEl: {
                    tag: 'span',
                    html: this.middleSymbol
                }
            },
            this.endDateField
        ]
    },

    afterInitComponent: Ext.emptyFn,

    generateStartDateField: function () {
        return Ext.widget('datefield', {
            name: this.startDateFieldName,
            flex: 1,
            value: this.startDate,
            maxValue: this.endDate,
            listeners: this.startDateListener
        });
    },

    generateEndDateFieldField: function () {
        return Ext.widget('datefield', {
            name: this.endDateFieldName,
            flex: 1,
            value: this.endDate,
            maxValue: this.startDate,
            listeners: this.endDateListener
        });
    },

    createFieldInteraction: function () {
        this.startDateField.addListener('change', function (field) {
            var eField = this.endDateField;
            eField.setMinValue(field.getValue());
            eField.validate();
        }, this);

        this.endDateField.addListener('change', function (field) {
            var sField = this.startDateField;
            sField.setMaxValue(field.getValue());
            sField.validate();
        }, this);
    }
});