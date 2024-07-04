Ext.define('WEBAPP.model.MmWhapldt', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'APPLY_DATE', type: 'string' },
        { name: 'APPLY_YEAR_MONTH', type: 'string' },
        { name: 'APPLY_DAY', type: 'string' },
        { name: 'WH_NO', type: 'string' },
        { name: 'WH_NO_N', type: 'string' },
        { name: 'WH_NO_OLD', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});