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

    var reportUrl = '/Report/C/CE0012.aspx';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var windowHeight = $(window).height();
    //var userId, userName, userInid, userInidName;
    var T1cell = '';
    var T1LastRec = null;
    var countData = null;

    var viewModel = Ext.create('WEBAPP.store.CE0012VM');

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
                            fieldCls: 'required',
                            multiSelect: false,
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
                            //format: 'Xm',
                            value: new Date()
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'P2',
                            id: 'P2',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 60,
                            width: 190,
                            padding: '0 4 0 4',
                            value: userId + ' ' + userName
                        },
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

                T1cell = 'cell';

                T1LastRec = record;

                //var f = T21Query.getForm();
                //f.findField('T21P0').setValue(record.data.WH_NAME);
                //f.findField('T21P1').setValue(record.data.CHK_YM);
                //f.findField('T21P2').setValue(record.data.CHK_PERIOD_NAME);
                //f.findField('T21P3').setValue(record.data.CHK_TYPE_NAME);
                //f.findField('T21P4').setValue(record.data.CHK_STATUS_NAME);
                //f.findField('T21P5').setValue(record.data.WH_KIND_NAME);
                //f.findField('T21P6').setValue(record.data.CHK_NO);
                //f.findField('T21P7').setValue(record.data.CHK_KEEPER);

                var f = T22Query.getForm();
                var value = { condition: '' };
                f.findField('condition').setValue(value);

                //T21Grid.setHeight(windowHeight - 150);

                var title = record.data.CHK_NO + ' ' + record.data.WH_NAME + ' ' + record.data.CHK_YM + ' ' + record.data.CHK_PERIOD_NAME + ' ' + record.data.WH_KIND_NAME

                T21Load();

                detailWindow.setTitle('盤點明細管理 ' + title);
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
    var T21Store = viewModel.getStore('Details');
    function T21Load() {
        
        T21Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T21Store.getProxy().setExtraParam("chk_level", T1LastRec.data.CHK_LEVEL);
        T21Store.getProxy().setExtraParam("condition", T22Query.getForm().findField('condition').getValue()['condition']);
        T21Store.getProxy().setExtraParam("wh_kind", T1LastRec.data.CHK_WH_KIND);

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

    var T22Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'panel',
            id: 'PanelP22',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'radiogroup',
                    name: 'condition',
                    fieldLabel: '顯示內容',
                    labelAlign: 'right',
                    items: [
                        { boxLabel: '全部', width: 60, name: 'condition', inputValue: '', checked: true },
                        { boxLabel: '盤盈', width: 60, name: 'condition', inputValue: '>' },
                        { boxLabel: '盤虧', width: 60, name: 'condition', inputValue: '<' }
                    ],
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            T21Load();
                        }
                    }
                },
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
                text: '列印盤點差異分析報表',
                id: 'btnPrint',
                name: 'btnPrint',
                handler: function () {
                    showReport();
                }
            },
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
                + '&CONDITION=' + T22Query.getForm().findField('condition').getValue()['condition']
                + '&WH_KIND=' + T1LastRec.data.CHK_WH_KIND
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
        height: windowHeight - 35,
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
                text: "中/英 文名稱",
                dataIndex: 'MMNAME',
                width: 150
            },
            {
                text: "庫存量",
                dataIndex: 'STORE_QTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "庫存成本",
                dataIndex: 'STORE_COST',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點數量",
                dataIndex: 'CHK_QTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "實盤成本",
                dataIndex: 'CHK_COST',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "差異百分比",
                dataIndex: 'DIFF_P',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{DIFF_P} %',
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "差異成本",
                dataIndex: 'DIFF_COST',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "差異分析說明",
                dataIndex: 'DIFF_REMARK',
                width: 90,
            },
            {
                header: "",
                flex: 1
            }
        ]
    });
    var detailWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    T22Query,
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
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點明細管理",
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
        T21Grid.setHeight(windowHeight - 35);
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
});