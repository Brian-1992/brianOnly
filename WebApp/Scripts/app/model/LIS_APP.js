
Ext.define('WEBAPP.model.LIS_APP', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PURCHNO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'APPQTY', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'APPUSER', type: 'string' },
        { name: 'APPTIME', type: 'string' },
        { name: 'APPLY_NOTE', type: 'string' },
        { name: 'INSTIME', type: 'string' },
        { name: 'RDTIME', type: 'string' },
        { name: 'DOCNO', type: 'string' },
        { name: 'REJ_NOTE', type: 'string' }
    ]
});