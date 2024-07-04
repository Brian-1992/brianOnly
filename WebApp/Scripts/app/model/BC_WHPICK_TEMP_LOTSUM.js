Ext.define('WEBAPP.model.BC_WHPICK_TEMP_LOTSUM', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'CALC_TIME', type: 'date' },
        { name: 'LOT_NO', type: 'string' },
        { name: 'COMPLEXITY_SUM', type: 'string' },
        { name: 'DOCNO_SUM', type: 'string' },
        { name: 'APPITEM_SUM', type: 'string' },
        { name: 'CALC_TIME_TEST_String', type: 'string' },
        { name: 'CALC_TIME_TEST_DateTime', type: 'date' }
    ]
});