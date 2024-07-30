Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec;

    var viewModel = Ext.create('WEBAPP.store.AA.AA0122VM');
    var T1Store = viewModel.getStore('ME_DOCM');

    function T1Load() {
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').checked ? 'Y' : 'N' );
        //T1Store.getProxy().setExtraParam("p3", session['Inid']);
        ////T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        //T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        //T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        //T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        //T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());
        //T1Store.getProxy().setExtraParam("FLOWID", T1Query.getForm().findField('FLOWID').getValue());
        T1Tool.moveFirst();
        //T1Store.loadPage(1);
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
    }

    var MatClassStore = viewModel.getStore('MAT_CLASS');
    function MatClassLoad() {
        MatClassStore.load({
            params: {
                start: 0
            }
        });
    }

    function getMonthData(date) {
        // 取得當前年份和月份
        const year = date.getFullYear() - 1911; // 轉換為民國年
        const month = date.getMonth() + 1; // 月份是從 0 開始，所以要加 1

        // 格式化成你需要的字串格式
        const yearMonth = `${year}${month.toString().padStart(2, '0')}`;

        // 取得上個月的年份和月份
        const lastMonthDate = new Date(date);
        lastMonthDate.setMonth(lastMonthDate.getMonth() - 1);
        const lastYear = lastMonthDate.getFullYear() - 1911;
        const lastMonth = lastMonthDate.getMonth() + 1;

        const lastMonthFormatted = `${lastYear}${lastMonth.toString().padStart(2, '0')}`;

        return { yearMonth, lastMonth: lastMonthFormatted };
    }
    var monthData = getMonthData(new Date());
    var thisMonth = monthData.yearMonth;
    var lastMonth = monthData.lastMonth;

    var YmStore = viewModel.getStore('YM');
    //function YmStoreLoad() {
    //    YmStore.load();
    //}

    var recordsToAdd = [
        { VALUE: thisMonth, TEXT: thisMonth },
        { VALUE: lastMonth, TEXT: lastMonth },
    ];

    YmStore.add(recordsToAdd);

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        //xtype: 'displayfield',
                        xtype: 'combo',
                        fieldLabel: '調帳年月',
                        name: 'P1',
                        id: 'P1',
                        store: YmStore,
                        editable: true,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        labelWidth: 60,
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '物料分類',
                        name: 'P2',
                        id: 'P2',
                        store: MatClassStore,
                        labelWidth: 90,
                        //editable: false,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        anyMatch: true,
                        typeAhead: true,
                        forceSelection: true,
                        queryMode: 'local',
                        triggerAction: 'all',
                    },
                    {
                        xtype: 'fieldcontainer',
                        fieldLabel: '目前戰備單價≠民最小單價',
                        defaultType: 'checkboxfield',
                        labelWidth: 200,
                        labelSeparator: '',
                        width: 230,
                        items: [
                            {
                                name: 'P3',
                                inputValue: '1',
                                checked: true
                            }
                        ]
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            T1Grid.getSelectionModel().deselectAll();
                            T1Load();
                            msglabel('');
                        }
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('');
                        }
                    },
                    {
                        xtype: 'button',
                        itemId: 'export',
                        text: '匯出',
                        id: 'export',
                        name: 'export',
                        handler: function () {
                            var p1 = T1Query.getForm().findField('P1').getValue();
                            var p2 = T1Query.getForm().findField('P2').getValue();
                            var p3 = T1Query.getForm().findField('P3').checked ? 'Y' : 'N' ;
                            var p = new Array();

                            p.push({ name: 'P1', value: p1 });
                            p.push({ name: 'P2', value: p2 });
                            p.push({ name: 'P3', value: p3 });
                            PostForm('../../../api/AA0122/Excel', p);
                        }
                    },
                    {
                        xtype: 'button',
                        text: '單價調整',
                        handler: function () {
                            Ext.MessageBox.confirm('訊息', '如需執行換入換出作業須於同月執行<br>是否確定更新戰備單價<br>', function (btn, text) {
                                if (btn === 'yes') {
                                    //var data = { item: [] };
                                    //data.item.push(T1LastRec.data["DOCNO"]);
                                    Ext.Ajax.request({
                                        url: '/api/AA0122/UpdateStatusBySp',
                                        method: reqVal_p,
                                        params: {
                                            YM: T1Query.getForm().findField('P1').getValue(),
                                            MATCLASS: T1Query.getForm().findField('P2').getValue()
                                        },
                                        //async: true,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                msglabel('調帳成功');
                                                //T2Store.removeAll();
                                                //T1Grid.getSelectionModel().deselectAll();
                                                //T1Load();
                                                //Ext.getCmp('btnSubmit').setDisabled(true);
                                            } else {
                                                Ext.Msg.alert('錯誤', data.msg);
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                }
                            });
                        }
                    },
                ]
            }
        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?'
                    + 'ym=' + T1Query.getForm().findField('P1').getValue()
                    + '&mat_class=' + T1Query.getForm().findField('P2').getValue()
                    + '&inid=' + session['Inid'] + ' ' + session['InidName']
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
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
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "廠牌",
                dataIndex: 'M_AGENLAB',
                width: 70,
                align: 'left',
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "是否庫備",
                dataIndex: 'M_STOREID',
                width: 100,
                align: 'left',
            },
            {
                text: "目前戰備單價",
                dataIndex: 'MIL_PRICE',
                width: 100,
                style:'text-align:left',
                align: 'right',
            },
            {
                text: "民最小單價",
                dataIndex: 'UPRICE',
                width: 100,
                style: 'text-align:left',
                align: 'right',
            },
            {
                text: "調整後戰備單價",
                dataIndex: 'UPRICE',
                width: 110,
                style: 'text-align:left',
                align: 'right',
            },
            {
                text: "戰備庫存量",
                dataIndex: 'INV_QTY',
                width: 100,
                style: 'text-align:left',
                align: 'right',
            },
            //{
            //    text: "金額",
            //    dataIndex: 'TOTAL_PRICE',
            //    width: 70,
            //    align: 'right',
            //},
            {
                header: "",
                flex: 1
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
            split: true  //可以調整大小
        },
        items: [
            {

                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        //height: '50%',
                        split: true,
                        items: [T1Grid]
                    }
                ]
            }
        ]
    });

    var dateObj = new Date();
    var year = parseInt(dateObj.getFullYear()) - 1911;
    var month = parseInt(dateObj.getMonth()) + 1;
    if (parseInt(month) < 10)
        month = '0' + month;
    T1Query.getForm().findField('P1').setValue(year + '' + month);
    T1Query.getForm().findField('P2').setValue('02');
});