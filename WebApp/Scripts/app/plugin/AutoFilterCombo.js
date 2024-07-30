Ext.define('MMIS.plugin.AutoFilterCombo', {
    extend: 'Ext.AbstractPlugin',
    alias: 'plugin.autofiltercombo',

    init: function (combo) {
        var me = this;
        combo.on('beforequery', function (e) {
            me.filterValue(combo, e.query);
            return false;
        });
    },

    filterValue: function (combo, value) {
        var store = combo.store,
            oFilters = store.filters.clone();

        acFilter = Ext.create('Ext.util.Filter', {
            id: 'acFilter',
            filterFn: function (record, id) {
                var text = record.get(combo.displayField);
                return (text.toUpperCase().indexOf(value.toUpperCase()) != -1);
            }
        });

        oFilters.replace(acFilter);
        store.clearFilter();
        store.filter(oFilters.getRange());

        combo.expand();
    }
});