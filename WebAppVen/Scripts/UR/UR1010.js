Ext.onReady(function () {
    var T1Get = '../../../api/UR1010/ALL';
    var TabLogRefCombo = '../../../api/UR1010/GetTabLogRefCombo';
    var FieldNameCombo = '../../../api/UR1010/GetFieldNameCombo';
    var T1Name = '';

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    var table_key = Ext.getUrlParam('TABLE_KEY');
    var table_key_value = Ext.getUrlParam('TABLE_KEY_VALUE');
    var field_name = Ext.getUrlParam('FIELD_NAME');
    var T1Name = Ext.getUrlParam('wTitle');
    var isHidden = (Ext.getUrlParam('TABLE_KEY_VALUE') == undefined) ? false : true;
    var ComboStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM','VALUE']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: TabLogRefCombo,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var store = data.etts;
                    if (store.length > 0) {
                        for (var i = 0; i < store.length; i++) {
                            ComboStore.add({ KEY_CODE: store[i].KEY_CODE, COMBITEM: store[i].COMBITEM,VALUE: store[i].VALUE});
                        }
                        if (table_key != undefined) {
                            var index = ComboStore.find('KEY_CODE', table_key);
                            if (index != -1) {
                                T1Query.getForm().findField('P0').setValue(ComboStore.data.items[index].data.KEY_CODE);
                                FieldComboStore.load();
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {
                //alert(response.responseText);
            }
        });
    }
    setComboData();
    var FieldComboStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM'],
        pageSize: 9999,
        autoLoad: false,
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                //store.insert(0, { KEY_CODE: '', COMBITEM: '' });
                if (field_name != undefined) {
                    var index = store.find('KEY_CODE', field_name);
                    if (index != -1) {
                        T1Query.getForm().findField('P2').setValue(store.data.items[index].data.KEY_CODE);
                    }
                }
                if (table_key_value != undefined) {
                    T1Query.getForm().findField('P1').setValue(table_key_value);
                    T1Store.loadPage(1);
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: FieldNameCombo,
            reader: {
                type: 'json',
                root: 'etts'
            }
        }
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['PROC_DT', 'FIELD_NAME', 'NEW_VALUE', 'OLD_VALUE', 'PROC_USER','PROC_IP']
    });

    function T1Load() {
        if (T1Query.getForm().findField('P0').getValue() == null | T1Query.getForm().findField('P0').getValue() == "") {
            alert('請選擇表格!');
            msglabel('請選擇表格!'); 
            T1Query.getForm().findField('P0').focus();
        } else if (T1Query.getForm().findField('P1').getValue() == null | T1Query.getForm().findField('P1').getValue() == "") {
            alert('請輸入值!');
            msglabel('請輸入值!');
            T1Query.getForm().findField('P1').focus();
        } else {
            msglabel('訊息區：');
            T1Store.loadPage(1);
        }

    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    function getFieldCombo() {
        if (T1Query.getForm().findField('P0').getValue() == null | (T1Query.getForm().findField('P0').getValue() == ""))
        {
            alert('請選擇表格!');
            msglabel('請選擇表格!');
        } else {
            FieldComboStore.load();
        }
    }

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'PROC_DT', direction: 'DESC' }, { property: 'FIELD_NAME', direction: 'asc' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T1Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    Ext.define('comboSelectedCount', {
        alias: 'plugin.selectedCount',
        init: function (combo) {

            var fl = combo.getFieldLabel();

            combo.on({
                select: function (me, records) {

                    var len = records.length,
                        store = combo.getStore();

                    // toggle all selections
                    Ext.each(records, function (obj, i, recordsItself) {
                        if (records[i].data.KEY_CODE === 'ALL') {
                            len = store.getCount();
                            combo.select(store.getRange());
                        }
                    });

                    me.setFieldLabel(fl + ' (' + len + ' selected)');
                },
                beforedeselect: function (me, record, index) {
                    me.setFieldLabel(fl);
                }
            })
        }
    });
    //var comSelectValues = [];//保存被選中的選項
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 50
        },
        hidden:isHidden,
        border: false,
        items: [{
                xtype: 'combo',
                store: ComboStore,
                displayField: 'COMBITEM',
                valueField: 'KEY_CODE',
                id: 'P0',
                name: 'P0',
                fieldLabel: '表格',
                queryMode: 'local',
                editable: false,
                matchFieldWidth: false,
                listConfig: {
                    width: 200
                },
                emptyText: "------請選擇------",
                autoSelect: true,
                listeners: {
                    select: function (combo, record, index) {
                        Ext.getCmp('P2').setValue("");
                        Ext.getCmp('P2').setRawValue("");
                        var t = (record[0].data.VALUE==null) ?"":record[0].data.VALUE;
                        Ext.getCmp('Remark').setText(t);
                        T1Query.getForm().findField('P1').setValue("");
                        getFieldCombo();
                    }
                }
        }, {
            xtype: 'textfield',
            fieldLabel: '值',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 60
        }, {
            xtype: 'splitter'
        }, {
            xtype: 'label',
            //width: 140,
            id: 'Remark'
        },{
                xtype: 'combo',
                store: FieldComboStore,
                displayField: 'COMBITEM',
                valueField: 'KEY_CODE',
                id: 'P2',
                name: 'P2',
                fieldLabel: '欄位',
                queryMode: 'local',
                multiSelect: true,
                matchFieldWidth: true
                //matchFieldWidth: false,
                ////1...................
                //listConfig: {
                //    width: 200,
                //    itemTpl: Ext.create('Ext.XTemplate', '<input type=checkbox>{[values.COMBITEM]}'),
                //    onItemSelect: function (record) {
                //        var node = this.getNode(record);
                //        if (node) {
                //            Ext.fly(node).addCls(this.selectedItemCls);
                //            var checkboxs = node.getElementsByTagName("input");
                //            if (checkboxs != null)
                //                var checkbox = checkboxs[0];
                //            checkbox.checked = true;
                //            if (record.data.KEY_CODE == "ALL") {
                //                var nodes = this.getNodes(record.store.data.items);
                //                var check_boxs = [];
                //                var checkAll_box = null;
                //                Ext.each(nodes, function (item) {
                //                    var check_box = item.getElementsByTagName("input");
                //                    check_boxs.push(check_box[0]);
                //                    if (item.innerText == AllCheckBox.name) {
                //                        checkAll_box = check_box[0];
                //                    }
                //                });
                //            }
                //        }
                //    },
                //    listeners: {
                //        itemclick: function (view, record, item, index, e, eOpts) {
                //            var isSelected = view.isSelected(item);
                //            var checkboxs = item.getElementsByTagName("input");
                //            if (checkboxs != null) {
                //                var checkbox = checkboxs[0];
                //                if (!isSelected) {
                //                    checkbox.checked = true;
                //                } else {
                //                    checkbox.checked = false;
                //                }
                //            }
                //        }
                //    }
                //}
                //1..................
                //,listConfig: {
                //    //width: 200,
                //    itemTpl: Ext.create('Ext.XTemplate', '<input type=checkbox>{[values.COMBITEM]}'),
                //    onItemSelect: function (record) {
                //        var node = this.getNode(record);
                //        if (node) {
                //            Ext.fly(node).addCls(this.selectedItemCls);
                //            var checkboxs = node.getElementsByTagName("input");
                //            if (checkboxs != null)
                //                var checkbox = checkboxs[0];
                //            checkbox.checked = true;
                //        }
                //    }, listeners: {
                //        beforeshow: function(picker) {
                //            picker.minWidth = picker.up('combo').getSize().width;
                //        },
                //        itemclick: function (view, record, item, index, e, eOpts) {
                //            var AllCheckBox = {
                //                KEY_CODE: "",
                //                COMBITEM: "全部"
                //            };
                //            var nodes = this.getNodes(record.store.data.items);
                //            var check_boxs = [];
                //            var checkAll_box = null;
                //            Ext.each(nodes, function (item) {
                //                var check_box = item.getElementsByTagName("input");
                //                check_boxs.push(check_box[0]);
                //                if (item.innerText == AllCheckBox.COMBITEM) {
                //                    checkAll_box = check_box[0];
                //                }
                //            });
                //            var isSelected = view.isSelected(item);
                //            var isClickAll = false;
                //            if (record.data.KEY_CODE == AllCheckBox.KEY_CODE) {
                //                isClickAll = true;
                //            }
                //            if (isClickAll && !isSelected) {//點全選框與全選框沒有被選中
                //                Ext.each(check_boxs, function (item) {
                //                    item.checked = true;
                //                });
                //                var newValues = [];
                //                Ext.each(record.store.data.items, function (item) {
                //                    newValues.push(item.data.KEY_CODE);
                //                });
                //                comSelectValues = newValues;
                //            } else if (isClickAll) {//點全選框
                //                Ext.each(check_boxs, function (item) {
                //                    item.checked = false;
                //                });
                //                comSelectValues = [];
                //            } else {
                //                var dex = comSelectValues.indexOf(AllCheckBox.KEY_CODE);
                //                if (dex >= 0) {//全選框被選中
                //                    var cancelChecks = [AllCheckBox.KEY_CODE, record.data.KEY_CODE];
                //                    Ext.each(cancelChecks, function (item) {
                //                        var cancel_dex = comSelectValues.indexOf(item);
                //                        if (cancel_dex >= 0) {
                //                            comSelectValues.splice(cancel_dex, 1);
                //                        }
                //                    });
                //                    checkAll_box.checked = false;
                //                } else {
                //                    if (!isSelected) {
                //                        comSelectValues.push(record.data.KEY_CODE);
                //                    } else {
                //                        var cancel_dex = comSelectValues.indexOf(record.data.KEY_CODE);
                //                        if (cancel_dex >= 0) {
                //                            comSelectValues.splice(cancel_dex, 1);
                //                        }
                //                    }
                //                }
                //                //-------------
                //                var checkboxs = item.getElementsByTagName("input");
                //                if (checkboxs != null) {
                //                    var checkbox = checkboxs[0];
                //                    if (!isSelected) {
                //                        checkbox.checked = true;
                //                    } else {
                //                        checkbox.checked = false;
                //                    }
                //                }
                //            }
                //            if (comSelectValues.length + 1 == record.store.data.items.length) {//檢查是否是除全選框外的其它選項都被選中
                //                //check_boxs[0].checked = true;
                //                comSelectValues.push(AllCheckBox.KEY_CODE);
                //                //var combo = Ext.getCmp('P2');
                //                //combo.setValue("全選");
                //                //combo.setRawValue("");
                //                //T1Query.getForm().findField('P2').select("全選");
                //                //T1Query.getForm().findField('P2').select("全選");
                //                //var newValues = [];
                //                //Ext.each(record.store.data.items, function (item) {
                //                //    newValues.push(item.data.KEY_CODE);
                //                //});
                //                //me.selectValues = newValues;
                //            }
                //        }
                //    }
                //}
                //2.........
            }, {
                xtype: 'button',
                text: '查詢',
                iconCls: 'TRASearch',
                padding: '2 4 2 4',
                handler: T1Load
            }, {
                xtype: 'button',
                text: '清除',
                iconCls: 'TRAClear',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus();
                }
            }]      
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {
            afterrender: function (T1Tool) {
                T1Tool.emptyMsg = '<font color=red>沒有任何資料</font>';
            }
        }
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }
        ],

        // grid columns
        columns: [{
            text: "日期",
            dataIndex: 'PROC_DT',
            width: 130
        }, {
            text: "欄位",
            dataIndex: 'FIELD_NAME',
            width: 100
        }, {
            text: "新",
            dataIndex: 'NEW_VALUE',
            flex: 0.5
        }, {
            text: "舊",
            dataIndex: 'OLD_VALUE',
            flex: 0.5
        }, {
            text: "異動人員",
            dataIndex: 'PROC_USER',
            width: 100
        }, {
            text: "用戶端IP",
            dataIndex: 'PROC_IP',
            width: 110
        }
        ]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            split: true,
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        }
        ]
    });
});
