Ext.define('WEBAPP.model.DGMISS_MAST', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'INID', type: 'string' },
        { name: 'INID_T', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});