Ext.define('WEBAPP.model.MiUnitexch', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' },
        { name: 'UNIT_CODE', type: 'string' },
        { name: 'AGEN_NO', type: 'string' },
        { name: 'EXCH_RATIO', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});