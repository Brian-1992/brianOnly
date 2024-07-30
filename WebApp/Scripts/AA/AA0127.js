Ext.define('overrides.selection.CheckboxModel', {
    override: 'Ext.selection.CheckboxModel',

    getHeaderConfig: function () {
        var config = this.callParent();
        if (Ext.isFunction(this.selectable)) {

            config.selectable = this.selectable;
            config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                if (this.selectable(record)) {
                    record.selectable = false;
                    return '';
                }
                record.selectable = true;
                return this.defaultRenderer();
            };
            this.on('beforeselect', function (rowModel, record, index, eOpts) {
                return !this.selectable(record);
            }, this);
        }

        return config;
    },

    updateHeaderState: function () {
        // check to see if all records are selected
        var me = this,
            store = me.store,
            storeCount = store.getCount(),
            views = me.views,
            hdSelectStatus = false,
            selectedCount = 0,
            selected, len, i, notSelectableRowsCount = 0;

        if (!store.isBufferedStore && storeCount > 0) {
            hdSelectStatus = true;
            store.each(function (record) {
                if (!record.selectable) {
                    notSelectableRowsCount++;
                }
            }, this);
            selected = me.selected;

            for (i = 0, len = selected.getCount(); i < len; ++i) {
                if (store.indexOfId(selected.getAt(i).id) > -1) {
                    ++selectedCount;
                }
            }
            hdSelectStatus = storeCount === selectedCount + notSelectableRowsCount;
        }

        if (views && views.length) {
            me.column.setHeaderStatus(hdSelectStatus);
        }
    }
});



Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var St_MatclassGet = '../../../api/AA0069/GetMatclassCombo';
    var MMCODEComboGet = '../../../api/AA0061/GetMMCODECombo';
    var T1GetExcel = '../../../api/AA0061/Excel';



    var DeptQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            url: '/api/AA0127/GetDeptCombo',
            reader: {
                type: 'json',
                root: 'etts'
            }
        },
        autoLoad: true
    });

    var StatusQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        data: [
            { "VALUE": "B", "COMBITEM": "B - 確認回傳" },
            { "VALUE": "P", "COMBITEM": "P - 已過帳" },
            
        ]
    });




   
    
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                    xtype: 'datefield',
                    fieldLabel: '交易日期',
                    name: 'D0',
                    id: 'D0',
                    width: 160
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'D1',
                    id: 'D1',
                    labelWidth: 7,
                    width: 120
            

                }, {
                    xtype: 'combo',
                    fieldLabel: '部門',
                    name: 'P0',
                    id: 'P0',
                    store: DeptQueryStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                    autoSelect: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {
                            //var values = combo.up('form').getValues();
                            //var category = values.KINDDATARB;
                            //var level = values.P2;
                            //WhnoQueryStore.load({
                            //    params: {
                            //        category: category,
                            //        level: level
                            //    }
                            //});
                        }
                    },
                }, {
                    xtype: 'combo',
                    fieldLabel: '狀態',
                    name: 'P1',
                    id: 'P1',
                    store: StatusQueryStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    anyMatch: true,
                    autoSelect: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                }, {
                    
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load,
               
                },
               
            ],
        }],

    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['TXTDAY', 'MMCODE', 'AGEN_NO', 'DEPT', 'AGEN_NON', 'DEPTN', 'SBNO,', 'FBNO', 'AIR', 'XSIZE', 'STATUS', 'DOCNO','SEQ']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'MMCODE', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    //p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p0: T1Query.getForm().findField('D0').getValue(),
                    p1: T1Query.getForm().findField('D1').getValue(),
                    p2: T1Query.getForm().findField('P0').getValue(),
                    p3: T1Query.getForm().findField('P1').getValue(),
                   
                    //p8: T1QueryForm.getForm().findField('P8').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0127/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });


    function T1Load() {

        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        //displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '進貨核撥', //T1Query
                handler: function () {
                    var records = T1Grid.getSelectionModel().getSelection();
                    console.log(records)
                    //return;
                    if (records.length == 0) {
                        Ext.Msg.alert('訊息', '請先選擇進貨核撥資料!');

                        return;
                    }

                    Ext.MessageBox.confirm('訊息', '是否確定進貨核撥?', function (btn, text) {
                        if (btn === 'yes')
                            for (var i = 0; i < records.length; i++) {
                                if (records[i].data.DEPT==null)
                                {
                                    Ext.Msg.alert('訊息', '部門為空不可核撥!');
                                    return;
                                }

                                Ext.Ajax.request({
                                    actionMethods: {
                                        read: 'POST' // by default GET
                                    },
                                    url: '/api/AA0127/UpdateEnd',
                                    params: {
                                        txtday: records[i].data.TXTDAY,
                                        seq: records[i].data.SEQ,
                                        dept: records[i].data.DEPT,
                                        mmcode: records[i].data.MMCODE,
                                        fbno: records[i].data.FBNO,

                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.Msg.alert('訊息', '進貨核撥完成!');

                                            T1Store.load();
                                        }
                                    },
                                    failure: function (response, options) {

                                    }
                                });
                            }

                    });

                }
            },
     

        ]
    });
    
   
    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "更換日期",
                dataIndex: 'TXTDAY',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "廠商碼",
                dataIndex: 'AGEN_NON',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "放置位置",
                dataIndex: 'DEPTN',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "鋼號",
                dataIndex: 'SBNO',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "瓶號",
                dataIndex: 'FBNO',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "氣體",
                dataIndex: 'AIR',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "鋼瓶尺寸",
                dataIndex: 'XSIZE',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "狀態",
                dataIndex: 'STATUS',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "進貨文件單號",
                dataIndex: 'DOCNO',
                width: 180,
                renderer: function (val, meta, record) {
                    if (val != null)
                        return '<a href=javascript:popWinForm("' + record.data['DOCNO'] + '") >' + val + '</a>';
                }
                //renderer: function (val, meta, record) {
                //    if (val != null)
                //        return '<a href=javascript:popWinForm("' + record.data['MMCODE'] + '") >' + val + '</a>';
                //}
            }, {
                text: "SEQ",
                dataIndex: 'SEQ',
                style: 'text-align:left',
                align: 'left',
                hidden: true,
                width: 100
            }, {
                text: "AGEN_NO",
                dataIndex: 'AGEN_NO',
                style: 'text-align:left',
                align: 'left',
                hidden: true,
                width: 100
            }, {
                text: "DEPT",
                dataIndex: 'DEPT',
                style: 'text-align:left',
                align: 'left',
                hidden: true,
                width: 100
           
            }],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox

            selectable: function (record) {
                return record.get('STATUS') !== 'B';
            },

        },
        ),
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });
    var callableWin = null;
    popWinForm = function (docno) {
        var strUrl = "AA0127_Form?strParam=" + docno;
        //var strUrl = "AA0127_Form?strMmcode=" + strMmcode;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, '進貨核撥文件明細', viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
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
        },
        ]
    });

    T1Query.getForm().findField('D0').focus();

});