Ext.define('WEBAPP.model.BC_WHPICK_TEMP_LOTDOC', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'CALC_TIME', type: 'date' },
        { name: 'LOT_NO', type: 'string' },
        { name: 'DOCNO', type: 'string' }
    ]
});