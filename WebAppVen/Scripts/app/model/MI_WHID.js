Ext.define('WEBAPP.model.MI_WHID', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },        
        { name: 'WH_USERID', type: 'string' },       
        { name: 'TASK_ID', type: 'string' },       
        { name: 'CREATE_TIME', type: 'string' },     
        { name: 'CREATE_USER', type: 'string' },      
        { name: 'UPDATE_TIME', type: 'string' },       
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});