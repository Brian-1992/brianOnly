Ext.define('WEBAPP.model.COMBO_MODEL', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'VALUE', type: 'string' },         
        { name: 'TEXT', type: 'string' },  
        { name: 'COMBITEM', type: 'string' }, 
        { name: 'EXTRA1', type: 'string' },
        { name: 'EXTRA2', type: 'string' }
    ]
});