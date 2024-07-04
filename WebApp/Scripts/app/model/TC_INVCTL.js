Ext.define('WEBAPP.model.TC_INVCTL', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'M6AVG_USEQTY', type: 'string' },
        { name: 'M3AVG_USEQTY', type: 'string' },
        { name: 'M6MAX_USEQTY', type: 'string' },
        { name: 'M3MAX_USEQTY', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' }
    ]
});