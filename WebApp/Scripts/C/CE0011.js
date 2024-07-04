Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var WhnoComboGet = '../../../api/CE0002/GetWhnoCombo';
    var T1Name = '三盤(複盤)差異品項列表作業';

    var reportUrl = '/Report/C/CE0011.aspx';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var windowHeight = $(window).height();
    //var userId, userName, userInid, userInidName;
    var T1cell = '';
    var T1LastRec = null;

    var viewModel = Ext.create('WEBAPP.store.CE0011VM');

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                        }
                            T1Query.getForm().findField('P0').setValue(wh_nos[0].WH_NO);
                            T1Load(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1Store = viewModel.getStore('MasterAll');
    function T1Load(clearMsg) {
        
        if (clearMsg) {
            msglabel('');
        }

        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T1Store.getProxy().setExtraParam("p2", userId);
        T1Tool.moveFirst();


    }


    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
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
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            name: 'P0',
                            id: 'P0',
                            fieldLabel: '庫房代碼',
                            displayField: 'WH_NAME',
                            valueField: 'WH_NO',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            typeAhead: true,
                            //forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                        },
                        {
                            xtype: 'monthfield',
                            fieldLabel: '盤點年月',
                            name: 'P1',
                            id: 'P1',
                            //enforceMaxLength: true,
                            //maxLength: 5,
                            //minLength: 5,
                            //regexText: '請填入民國年月',
                            //regex: /\d{5,5}/,
                            labelWidth: mLabelWidth,
                            width: 180,
                            padding: '0 4 0 4',
                            //fieldCls: 'required',
                            //format: 'Xm',
                            value: new Date()
                        },
                        //{
                        //    xtype: 'displayfield',
                        //    fieldLabel: '負責人',
                        //    name: 'P2',
                        //    id: 'P2',
                        //    enforceMaxLength: true,
                        //    maxLength: 21,
                        //    labelWidth: 60,
                        //    width: 190,
                        //    padding: '0 4 0 4',
                        //    value: userId + ' ' + userName
                        //},
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                                    return;
                                }

                                T1Load(true);
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NAME',
                width: 120
            },
            {
                text: "盤點年月",
                dataIndex: 'CHK_YM',
                width: 70
            },
            {
                text: "庫房級別",
                dataIndex: 'CHK_WH_GRADE',
                width: 70
            },
            {
                text: "庫房分類",
                dataIndex: 'WH_KIND_NAME',
                width: 80
            },
            {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD_NAME',
                width: 70
            },
            {
                text: "盤點類別",
                //dataIndex: 'CHK_TYPE_NAME',
                dataIndex: 'CHK_TYPE',
                width: 100,
                xtype: 'templatecolumn',
                //renderer: function (val, meta, record) {
                //    return record.data.CHK_TYPE_NAME;
                //},
                tpl: '{CHK_TYPE_NAME}'
            },
            {
                text: "物料分類",
                //dataIndex: 'CHK_TYPE_NAME',
                dataIndex: 'CHK_CLASS',
                width: 100,
                xtype: 'templatecolumn',
                //renderer: function (val, meta, record) {
                //    return record.data.CHK_TYPE_NAME;
                //},
                tpl: '{CHK_CLASS_NAME}'
            },
            //{
            //    text: "盤點階段",
            //    dataIndex: 'CHK_LEVEL_NAME',
            //    width: 80,
            //},
            {
                text: "初盤單號",
                dataIndex: 'CHK_NO',
                width: 120,
                renderer: function (val, meta, record) {
                    var CHK_NO = record.data.CHK_NO;
                    return '<a href=javascript:void(0)>' + CHK_NO + '</a>';
                },
            },
            //{
            //    text: "負責人員",
            //    dataIndex: 'CHK_KEEPER_NAME',
            //    width: 120
            //},
            //{
            //    text: "狀態",
            //    dataIndex: 'CHK_STATUS_NAME',
            //    width: 80
            //},
            {
                text: "已完成盤點階段",
                dataIndex: 'FINAL_LEVEL',
                width: 100,
                renderer: function (val, meta, record) {
                    return getChkLevelName(record.data.FINAL_LEVEL);
                },
            },
            {
                text: "進行中盤點階段",
                dataIndex: 'ING_LEVEL',
                width: 100,
                renderer: function (val, meta, record) {
                    return getChkLevelName(record.data.ING_LEVEL);
                },
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            //click: {
            //    element: 'el',
            //    fn: function () {

            //    }
            //},
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('');

                //if (T1cell == '')
                
                T1Rec = record;
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                
                var columnIndex = self.getHeaderCt().getHeaderAtIndex(cellIndex).config.dataIndex;
                if (columnIndex != 'CHK_NO') {
                    return;
                }
                // T61LastRec = record;
                T1cell = 'cell';

                T1LastRec = record;

                var f = T21Query.getForm();
                f.findField('T21P0').setValue(record.data.WH_NAME);
                f.findField('T21P1').setValue(record.data.CHK_YM);
                f.findField('T21P2').setValue(record.data.CHK_PERIOD_NAME);
                f.findField('T21P3').setValue(record.data.CHK_TYPE_NAME);
                f.findField('T21P4').setValue(record.data.CHK_STATUS_NAME);
                f.findField('T21P5').setValue(record.data.WH_KIND_NAME);
                f.findField('T21P6').setValue(record.data.CHK_NO);
                f.findField('T21P7').setValue(record.data.CHK_KEEPER);

                //T21Grid.setHeight(windowHeight - 150);

                var title = record.data.CHK_NO + ' ' + record.data.WH_NAME + ' ' + record.data.CHK_YM + ' ' + record.data.CHK_PERIOD_NAME + ' ' + record.data.WH_KIND_NAME

                T21Load();
                
                detailWindow.setTitle('盤點明細管理 ' + title);

                //var chk_qty_column = Ext.getCmp('chk_qty_column');
                //var columnTitle = record.data.CHK_LEVEL == '1' ? '初' :
                //                  record.data.CHK_LEVEL == '2' ? '複' : '三';
                //chk_qty_column.setText(columnTitle + '盤量');

                detailWindow.show();
            },
        }
    });
    function getChkLevelName(level) {
        switch (level) {
            case '1':
                return level + ' 初盤';
                break;
            case '2':
                return level + ' 複盤';
                break;
            case '3':
                return level + ' 三盤';
                break;
            default:
                return '無';
        }
    }


    // ------ 盤點項目編輯 ------
    var T21Store = viewModel.getStore('DetailInclude');
    function T21Load() {
        T21Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T21Store.getProxy().setExtraParam("chk_level", T1LastRec.data.CHK_LEVEL);

        T21Tool.moveFirst();
    }
    var T21Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP21',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T21P0',
                            id: 'T21P0',
                            labelWidth: 60,
                            width: 180,
                            fieldLabel: '庫房代碼',
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'T21P1',
                            id: 'T21P1',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點期',
                            name: 'T21P2',
                            id: 'T21P2',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點類別',
                            name: 'T21P3',
                            id: 'T21P3',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '狀態',
                            name: 'T21P4',
                            id: 'T21P4',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP22',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點單號',
                            name: 'T21P6',
                            id: 'T21P6',
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'T21P5',
                            id: 'T21P5',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            fieldLabel: '庫房分類',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'T21P7',
                            id: 'T21P7',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                }
            ]
        }]
    });
    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '列印品項差異表',
                id: 'btnPrint',
                name: 'btnPrint',
                handler: function () {
                    showReport();
                }
            },
            '<span style="color:red">(扣庫品項列入差異，非扣庫品項列入消耗)</span>'
        ]
    });

    function showReport(record) {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?CHK_NO=' + T1LastRec.data.CHK_NO
                    + '&CHK_LEVEL=' + T1LastRec.data.CHK_LEVEL
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    var T21Grid = Ext.create('Ext.grid.Panel', {
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight - 60,
        //multiSelect: true,
        //viewConfig: {
        //    plugins: {
        //        ptype: 'gridviewdragdrop',
        //        dragGroup: 'firstGridDDGroup',
        //        dropGroup: 'secondGridDDGroup'
        //    },
        //    listeners: {
        //        //drop: function (node, data, dropRec, dropPosition) {
        //        //    
        //        //    var dropOn = dropRec ? ' ' + dropPosition + ' '
        //        //        + dropRec.get('name') : ' on empty view';
        //        //    var test = T21Grid.getStore();
        //        //}
        //        beforedrop: function(node, data, overModel, dropPosition, dropHandlers, eOpts){

        //        }
        //    }
        //},
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T21Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 90,
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTYC',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left',
                id: 'chk_qty_column'
            },
            {
                text: "差異量",
                dataIndex: 'QTY_DIFF',
                width: 60,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "消耗量",
                dataIndex: 'CONSUME_QTY',
                width: 60,
            },
            {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                width: 150,
            },
            {
                text: "盤點內容",
                dataIndex: 'STORE_LOC_NAMES',
                width: 150,
            }
        ],
        listeners: {
            load: function (self, records, successful, operation, eOpts) {
                
                var excludes = this.getStore();
            }

        }
    });

    var detailWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    //T21Query,
                    {
                        xtype: 'panel',
                        itemId: 't21Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        border: false,
                        //height: '50%',
                        //minHeight: windowHeight- 90,
                        items: [T21Grid]
                    }
                ],
            }
        ],
        //items: [T21Grid],
        //items: [T21Grid, T22Grid],
        width: "1000px",
        height: windowHeight,
        resizable: true,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點明細管理",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [{
            text: '關閉',
            handler: function () {
                detailWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                detailWindow.setY(0);
            }
        }
    });
    detailWindow.hide();
    // --------------------------

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            }
        ]
    });

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        detailWindow.setHeight(windowHeight);
        T21Grid.setHeight(windowHeight - 60);
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
});