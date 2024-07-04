Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Get = '/api/AA0160/All';
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "科室成本分攤比率維護";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var defaultColumns = [{
        xtype: 'rownumberer',
        width: 30,
        align: 'Center',
        labelAlign: 'Center'
    },
    {
        text: "責任中心",
        dataIndex: 'INID_NAME',
        width: 150
    }
    ];

    var st_ym = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0160/GetYmCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, options) {
                var storeCount = store.getCount();
                var combo_P0 = T1Query.getForm().findField('DATA_YM');
                if (storeCount > 0) {
                    combo_P0.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
    });

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 140;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, 
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '年月',
                    name: 'DATA_YM',
                    id: 'DATA_YM',
                    store: st_ym,
                    queryMode: 'local',
                    displayField: 'VALUE',
                    valueField: 'VALUE',
                    editable: false,
                    allowBlank: false,
                    fieldCls: 'required'
                }, {
                    xtype: 'hidden',
                    name: 'CAN_EDIT',
                    id: 'CAN_EDIT',
                    submitValue: false
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            T1Load();
                            msglabel('訊息區:');
                        }
                        else
                            Ext.Msg.alert('提醒', '年月為必填');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('DATA_YM').focus();
                        var combo_P0 = T1Query.getForm().findField('DATA_YM');
                        if (st_ym.getCount() > 0) {
                            combo_P0.setValue(st_ym.getAt(0).get('VALUE'));
                        }
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DATA_YM', type: 'string' },
            { name: 'INID', type: 'string' },
            { name: 'INID_NAME', type: 'string' },
            { name: 'EXTRA_DATA', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'INID', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件代入參數
                var np = {
                    p0: T1Query.getForm().findField('DATA_YM').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                // store載入完成後,將EXTRA_DATA解成額外欄位資料後再載入store內
                var reArray = [];
                var colName = [];
                for (var i = 2; i < T1Grid.columns.length; i++) {
                    colName.push(T1Grid.columns[i].dataIndex)
                }

                for (var i = 0; i < store.data.length; i++) {
                    // 每列的基本欄位
                    var itemObj = {
                        "DATA_YM": store.data.items[i].data['DATA_YM'],
                        "INID": store.data.items[i].data['INID'],
                        "INID_NAME": store.data.items[i].data['INID_NAME']
                    };
                    // 組合後面額外的欄位
                    var extraData = store.data.items[i].data['EXTRA_DATA'].split(',');
                    for (var j = 0; j < extraData.length; j++) {
                        if (extraData[j].split('.')[0])
                            itemObj[colName[j]] = extraData[j];
                        else
                            itemObj[colName[j]] = '0' + extraData[j];
                    }
                    
                    reArray.push(itemObj);
                }
                
                store.loadData(reArray);

                setEditable();
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'update',
                text: '更新',
                disabled: false,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var tempData = T1Grid.getStore().data.items;

                    var data = [];
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }
                    

                    if (data.length > 0) {
                        Ext.Ajax.request({
                            url: '/api/AA0160/UpdateDisratio',
                            method: reqVal_p,
                            contentType: "application/json",
                            params: {
                                EXTRA_DATA: Ext.util.JSON.encode(data)
                            },
                            success: function (response) {

                                myMask.hide();
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    msglabel('訊息區:資料修改成功');
                                    T1Tool.moveFirst();
                                }
                                else
                                    Ext.MessageBox.alert('錯誤', data.msg);
                            },

                            failure: function (response, action) {
                                myMask.hide();
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        });
                    }
                    else {
                        myMask.hide();
                        Ext.MessageBox.alert('訊息', '沒有需要更新的資料');
                    }
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        enableColumnMove: false,
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "責任中心",
                dataIndex: 'INID_NAME',
                width: 150
            }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                        if (T1Query.getForm().findField('CAN_EDIT').getValue() == 'Y') {
                            return true;
                        }
                        else {
                            return false;
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                        var chkValid = true;
                        var failMsg = '';
                        if (Number(context.value) > 100) {
                            chkValid = false;
                            failMsg = '比率不可大於100';
                        }

                        if (chkValid == false) {
                            Ext.MessageBox.alert('錯誤', failMsg);
                            context.cancel = true;
                            context.record.data[context.field] = context.originalValue;
                        }
                    }
                }
            })
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        // T1Grid.down('#update').setDisabled(T1Rec === 0);
    }

    function getDefaultColumns() {
        T1Grid.reconfigure(null, defaultColumns);

        Ext.Ajax.request({
            url: '/api/AA0160/GetColumns',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    if (data.etts.length > 0) {
                        var columns = data.etts;
                        setT1GridColumns(columns);
                    }

                    T1Store.removeAll();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setT1GridColumns(columns) {

        for (var i = 0; i < columns.length; i++) {
            var column = Ext.create('Ext.grid.column.Column', {
                text: columns[i].TEXT + '(' + columns[i].DATAINDEX + ')',
                dataIndex: columns[i].DATAINDEX,
                width: 120,
                align: 'right',
                style: 'text-align:left',
                sortable: false,
                editor: {
                    xtype: 'textfield',
                    regexText: '需為至多小數二位的數字',
                    regex: /^0?(100|(([1-9][0-9]{0,1})|([0-9]))(\.[0-9]{1,2})?)$/ // 用正規表示式限制可輸入內容
                }, align: 'right'
            });

            T1Grid.headerCt.insert(T1Grid.columns.length, column);
            T1Grid.columns.push(column);
        }
        T1Grid.getView().refresh();
    }

    // 當月及上月的資料才可修改
    function setEditable() {
        Ext.Ajax.request({
            url: '/api/AA0160/GetEditable',
            method: reqVal_p,
            contentType: "application/json",
            params: {
                DATA_YM: T1Query.getForm().findField('DATA_YM').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'Y') {
                        T1Grid.down('#update').setDisabled(false);
                        T1Query.getForm().findField('CAN_EDIT').setValue('Y');
                    }
                    else {
                        T1Grid.down('#update').setDisabled(true);
                        T1Query.getForm().findField('CAN_EDIT').setValue('N');
                    }   
                }
                else
                    Ext.MessageBox.alert('錯誤', data.msg);
            },
            failure: function (response, action) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }

    function T1Load() {

        msglabel('');
        T1Tool.moveFirst();
    }

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
            border: false,
            items: [T1Grid]
        }]
    });

    T1Query.getForm().findField('DATA_YM').focus();

    getDefaultColumns();  
});
