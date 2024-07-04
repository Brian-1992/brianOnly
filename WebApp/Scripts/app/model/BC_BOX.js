Ext.define('WEBAPP.model.BC_BOX', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'BOXNO', type: 'string' },
        { name: 'BARCODE', type: 'string' },
        { name: 'XCATEGORY', type: 'string' },
        { name: 'DESCRIPT', type: 'string' },
        { name: 'STATUS', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
    ]
});