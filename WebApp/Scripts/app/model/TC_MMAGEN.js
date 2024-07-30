Ext.define('WEBAPP.model.TC_MMAGEN', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' },
        { name: 'AGEN_NAMEC', type: 'string' },
        { name: 'PUR_UNIT', type: 'string' },
        { name: 'PUR_SEQ', type: 'string' },        
        { name: 'IN_PURPRICE', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});